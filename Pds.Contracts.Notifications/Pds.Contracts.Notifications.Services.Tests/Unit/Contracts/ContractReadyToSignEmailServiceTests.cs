using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Common.Identity.Enums;
using Pds.Core.DfESignIn.Exceptions;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using Pds.Core.Notification.Models;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.Contracts
{
    [TestClass, TestCategory("Unit")]
    public class ContractReadyToSignEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ContractReadyToSignEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IContractReadyToSignEmailService _contractReadyToSignEmailService;

        public ContractReadyToSignEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ContractReadyToSignEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _contractReadyToSignEmailService = new ContractReadyToSignEmailService(
                CreateAuthenticationService(),
                Options.Create(CreateContractsDataApiConfiguration()),
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _mockDfESignInPublicApi.Object,
                _mockAuditService.Object,
                _mockHttp.ToHttpClient());
        }


        [TestMethod]
        public async Task Process_WhenContractDataApiUnreachable_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            SetupServicesMock(null, queryString, isNotificationEmailQueueServiceThrowException: false, isContractDataApiThrowException: true);

            // Act
            Func<Task> actual = async () => { await _contractReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
        }

        [TestMethod]
        public async Task Process_WithInValidContract_DoesNotSendContractReadyToSignEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = ContractFundingType.Aebp,
                Status = ContractStatus.Approved,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, queryString);

            var expectedError = $"Contract id {mockContract.ContractDisplayText} doesn't meet the the criteria - Status [{mockContract.Status}], Amendmant type [{mockContract.AmendmentType}]. {nameof(ContractReadyToSignEmailMessage)} was not processed.";

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndFailedDSIApiCall_ThrowsException(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.None,
                Ukprn = 12345678
            };

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewContractsAndAgreements.ToString(), UserRole.SignContractsAndAgreements.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _contractReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();

            _mockHttp.VerifyNoOutstandingRequest();
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndInvalidRoles_DoesNotSendContractReadyToSignEmail(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Variation
            };

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError_SignContractsAndAgreements = string.Format(Constants.LogMessage, mockContract.ContractDisplayText, $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]");
            var expectedInformation_ViewContractsAndAgreements = string.Format(Constants.LogMessage, mockContract.ContractDisplayText, $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements] for organisation [{mockContract.Ukprn}]");

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError_SignContractsAndAgreements), Times.Once);
            _mockLogger.Verify(logger => logger.LogInformation(expectedInformation_ViewContractsAndAgreements), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndSignContractsAndAgreementsUserRole_SuccessfullySendsContractReadyToSignEmail(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Variation
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignEmail, documentType);

            var expectedAuditMessage = "ContractReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockContract, queryString);
            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString() });

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithBothValidUserRoles_SuccessfullySendsContractReadyToSignEmail(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Variation
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignEmail, documentType);

            var expectedAuditMessage = "ContractReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewContractsAndAgreements.ToString(), UserRole.SignContractsAndAgreements.ToString() });

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndViewContractsAndAgreementsUserRole_SuccessfullySendsContractReadyToSign_ViewOnly_Email(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Variation
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignViewOnlyEmail, documentType);

            var expectedAuditMessage = "ContractReadyToSignEmailMessage view only processed and published to SharedEmailprocessorQueue.";

            var expectedError_SignContractsAndAgreements = string.Format(Constants.LogMessage, mockContract.ContractDisplayText, $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]");

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError_SignContractsAndAgreements), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]")), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndTwoUsersWithSignContractAndAgreementsandViewContractsAndAgreementsUserRoles_SuccessfullySendsContractReadyToSignEmailAndContractReadyToSign_ViewOnly_Email(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Variation
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignEmail, documentType);
            var expectedMessage2 = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignViewOnlyEmail, documentType, true);

            var expectedAuditMessage = "ContractReadyToSignEmailMessage processed and published to SharedEmailprocessorQueue.";
            var expectedAuditMessage2 = "ContractReadyToSignEmailMessage view only processed and published to SharedEmailprocessorQueue.";

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() }, testTwoUsers: true);

            // Act
            await _contractReadyToSignEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage2)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage2)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndSignContractsAndAgreementsRoleAndNotificationServiceReturnsError_ThrowsException(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.None
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignEmail, documentType);

            SetupServicesMock(mockContract, queryString, isNotificationEmailQueueServiceThrowException: true);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString() });

            // Assert
            Func<Task> actual = async () => { await _contractReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();

            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Levy, "agreement")]
        [DataRow(ContractFundingType.Aebp, "contract")]
        public async Task Process_WithValidContractAndViewContractsAndAgreementsRoleAndNotificationServiceReturnsError_ThrowsException(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToSignEmailMessage()
            {
                ContractNumber = "Test-1001",
                VersionNumber = 1,
                Ukprn = 12345678
            };

            var queryString = new Dictionary<string, string>
            {
                { "contractNumber", serviceBusMessage.ContractNumber },
                { "versionNumber", serviceBusMessage.VersionNumber.ToString() },
                { "ukprn", serviceBusMessage.Ukprn.ToString() }
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = contractFundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.None
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReadyToSignViewOnlyEmail, documentType);

            SetupServicesMock(mockContract, queryString, isNotificationEmailQueueServiceThrowException: true);

            SetupDfESignInPublicApiMock(new[] { UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            Func<Task> actual = async () => { await _contractReadyToSignEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();

            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static ContractsDataApiConfiguration CreateContractsDataApiConfiguration()
            => new ()
            {
                ApiBaseAddress = TestBaseAddress
            };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, string messageType, string documentType, bool testSecondUser = false)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_Contracts,
                EmailAddresses = new[] { !testSecondUser ? "testemail" : "testemail2" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "DocumentTitle",  documentTitle },
                        { "contract or agreement", documentType }
                    }
                }
            };
        }

        private static IAuthenticationService<ContractsDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<ContractsDataApiConfiguration>>(MockBehavior.Strict);
            mockAuthenticationService
                .Setup(x => x.GetAccessTokenForAAD())
                .Returns(Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService.Object;
        }

        private void SetupDfESignInPublicApiMock(string[] roles, bool noUsersWithGivenRoles = false, bool isDfESignInPublicApiThrowsException = false, bool testTwoUsers = false)
        {
            var users = new List<UserContact>();

            if (!noUsersWithGivenRoles)
            {
                if (!testTwoUsers)
                {
                    users.Add(new UserContact()
                    {
                        Email = "testemail",
                        FirstName = "testFirstName",
                        LastName = "testLastName",
                        Roles = roles
                    });
                }
                else
                {
                    users.Add(new UserContact()
                    {
                        Email = "testemail",
                        FirstName = "testFirstName",
                        LastName = "testLastName",
                        Roles = new[] { roles[0] }
                    });
                    users.Add(new UserContact()
                    {
                        Email = "testemail2",
                        FirstName = "testFirstName2",
                        LastName = "testLastName2",
                        Roles = new[] { roles[1] }
                    });
                }
            }

            var mockUserContactsResponse = new UserContactLookupResponse
            {
                Users = users
            };

            if (!isDfESignInPublicApiThrowsException)
            {
                _mockDfESignInPublicApi
                    .Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Returns(Task.FromResult(mockUserContactsResponse));
            }
            else
            {
                _mockDfESignInPublicApi
                    .Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Throws(new GetApiResultException());
            }
        }

        private void SetupServicesMock(Contract mockContract, IDictionary<string, string> queryString, bool isNotificationEmailQueueServiceThrowException = false, bool isContractDataApiThrowException = false)
        {
            var response = new StringContent(JsonConvert.SerializeObject(mockContract), System.Text.Encoding.UTF8, "application/json");

            var config = CreateContractsDataApiConfiguration();

            if (isNotificationEmailQueueServiceThrowException)
            {
                _mockNotificationEmailQueueService
                    .Setup(service => service.SendAsync(It.IsAny<NotificationMessage>()))
                    .ThrowsAsync(new Exception());
            }
            else
            {
                _mockNotificationEmailQueueService
                    .Setup(service => service.SendAsync(It.IsAny<NotificationMessage>()))
                    .Returns(Task.CompletedTask);
            }

            _mockAuditService
                .Setup(service => service.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask);

            if (isContractDataApiThrowException)
            {
                _mockHttp
                    .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractDetailsEndpoint)
                    .WithQueryString(queryString)
                    .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                    .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractDetailsEndpoint)
                    .WithQueryString(queryString)
                    .Respond(HttpStatusCode.OK, response);
            }

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
