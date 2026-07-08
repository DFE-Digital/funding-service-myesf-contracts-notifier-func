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
    public class ContractReadyToReviewEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ContractReadyToReviewEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IContractReadyToReviewEmailService _contractReadyToReviewEmailService;

        public ContractReadyToReviewEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ContractReadyToReviewEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _contractReadyToReviewEmailService = new ContractReadyToReviewEmailService(
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
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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

            SetupServicesMock(null, queryString, false, true);

            // Act
            Func<Task> actual = async () => { await _contractReadyToReviewEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
        }

        [TestMethod]
        public async Task Process_WithInvalidContractOrAgreement_DoesNotSendContractOrAgreementReadyToReviewEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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
                Status = ContractStatus.Replaced,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, queryString);

            var expectedError = $"Contract id {mockContract.ContractDisplayText} doesn't meet the the criteria - Funding type: [{mockContract.FundingType}] Status: [{mockContract.Status}] Amendmant type [{mockContract.AmendmentType}]. {nameof(ContractReadyToReviewEmailMessage)} was not processed.";

            // Act
            await _contractReadyToReviewEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidContractOrAgreementAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _contractReadyToReviewEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();

            _mockHttp.VerifyNoOutstandingRequest();
        }

        [TestMethod]
        public async Task Process_WithValidContractOrAgreementAndInvalidRoles_DoesNotSendContractOrAgreementReadyToReviewEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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
                AmendmentType = ContractAmendmentType.Notfication,
                Ukprn = 12345678
            };

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError = string.Format(
                Constants.LogMessage,
                mockContract.ContractDisplayText,
                $"{nameof(ContractReadyToReviewEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]");

            // Act
            await _contractReadyToReviewEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ContractReadyToReviewEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}]")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, "contract")]
        [DataRow(ContractFundingType.Levy, "agreement")]
        public async Task Process_WithValidContractOrAgreementAndRequiredUserRoles_SuccessfullySendsContractOrAgreementReadyToReviewEmail(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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
                Status = ContractStatus.Approved,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, documentType, Constants.MessageType_ContractReadyToReviewEmail);

            var expectedAuditMessage = "ContractReadyToReviewEmailMessage processed and published to SharedEmailprocessorQueue.";


            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            await _contractReadyToReviewEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, "contract")]
        [DataRow(ContractFundingType.Levy, "agreement")]
        public async Task Process_WithValidContractOrAgreementRequiredUserRolesAndNotificationServiceReturnsError_ThrowsException(ContractFundingType contractFundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReadyToReviewEmailMessage()
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
                Status = ContractStatus.Approved,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, documentType, Constants.MessageType_ContractReadyToReviewEmail);

            SetupServicesMock(mockContract, queryString, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _contractReadyToReviewEmailService.Process(serviceBusMessage));

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static ContractsDataApiConfiguration CreateContractsDataApiConfiguration()
            => new ()
            {
                ApiBaseAddress = TestBaseAddress
            };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, string documentType,  string messageType)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_Contracts,
                EmailAddresses = new[] { "testemail" }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "DocumentTitle",  documentTitle },
                        { "contract or agreement",  documentType }
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

        private void SetupDfESignInPublicApiMock(string[] roles, bool noUsersWithGivenRoles = false, bool isDfESignInPublicApiThrowsException = false)
        {
            var users = new List<UserContact>();

            if (!noUsersWithGivenRoles)
            {
                users.Add(new UserContact()
                {
                    Email = "testemail",
                    FirstName = "testFirstName",
                    LastName = "testLastName",
                    Roles = roles
                });
            }

            var mockUserContactsResponse = new UserContactLookupResponse
            {
                Users = users
            };

            if (!isDfESignInPublicApiThrowsException)
            {
                _mockDfESignInPublicApi.Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
                    .Returns(Task.FromResult(mockUserContactsResponse));
            }
            else
            {
                _mockDfESignInPublicApi.Setup(x => x.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()))
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
