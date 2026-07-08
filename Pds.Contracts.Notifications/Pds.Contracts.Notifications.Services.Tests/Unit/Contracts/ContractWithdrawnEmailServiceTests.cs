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
    public class ContractWithdrawnEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ContractWithdrawnEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly IContractWithdrawnEmailService _contractWithdrawnEmailService;

        public ContractWithdrawnEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ContractWithdrawnEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _contractWithdrawnEmailService = new ContractWithdrawnEmailService(
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
            var serviceBusMessage = new ContractWithdrawnEmailMessage()
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
            Func<Task> actual = async () => { await _contractWithdrawnEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
        }

        [DataTestMethod]
        [DataRow(ContractStatus.WithdrawnByProvider)]
        [DataRow(ContractStatus.WithdrawnByAgency)]
        public async Task Process_WithValidContractOrAgreementAndFailedDSIApiCall_ThrowsException(ContractStatus contractStatus)
        {
            // Arrange
            var serviceBusMessage = new ContractWithdrawnEmailMessage()
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
                Status = contractStatus,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _contractWithdrawnEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();

            _mockHttp.VerifyNoOutstandingRequest();
        }

        [DataTestMethod]
        [DataRow(ContractStatus.WithdrawnByProvider, Constants.MessageType_ContractWithdrawnByProviderEmail)]
        [DataRow(ContractStatus.WithdrawnByAgency, Constants.MessageType_ContractWithdrawnByESFAEmail)]
        public async Task Process_WithValidContractOrAgreementAndInvalidRoles_DoesNotSendContractOrAgreementWithdrawnEmail(ContractStatus contractStatus, string messageType)
        {
            // Arrange
            var serviceBusMessage = new ContractWithdrawnEmailMessage()
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
                Status = contractStatus,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError = string.Format(
                Constants.LogMessage,
                mockContract.ContractDisplayText,
                $"{nameof(ContractWithdrawnEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}].");

            // Act
            await _contractWithdrawnEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ContractWithdrawnEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}].")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, ContractStatus.WithdrawnByProvider, Constants.MessageType_ContractWithdrawnByProviderEmail, "contract")]
        [DataRow(ContractFundingType.Levy, ContractStatus.WithdrawnByAgency, Constants.MessageType_ContractWithdrawnByESFAEmail, "agreement")]
        public async Task Process_WithValidContractOrAgreementAndRequiredUserRoles_SuccessfullySendsContractOrAgreementWithdrawnEmail(ContractFundingType contractFundingType, ContractStatus contractStatus, string messageType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractWithdrawnEmailMessage()
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
                Status = contractStatus,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, documentType, messageType);

            var expectedAuditMessage = "ContractWithdrawnEmailMessage processed and published to SharedEmailprocessorQueue.";


            SetupServicesMock(mockContract, queryString);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            await _contractWithdrawnEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, ContractStatus.WithdrawnByProvider, Constants.MessageType_ContractWithdrawnByProviderEmail, "contract")]
        [DataRow(ContractFundingType.Levy, ContractStatus.WithdrawnByAgency, Constants.MessageType_ContractWithdrawnByESFAEmail, "agreement")]
        public async Task Process_WithValidContractOrAgreementRequiredUserRolesAndNotificationServiceReturnsError_ThrowsException(ContractFundingType contractFundingType, ContractStatus contractStatus, string messageType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractWithdrawnEmailMessage()
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
                Status = contractStatus,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, documentType, messageType);

            SetupServicesMock(mockContract, queryString, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            Func<Task> actual = async () => { await _contractWithdrawnEmailService.Process(serviceBusMessage); };

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

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, string documentType, string messageType)
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
                        { "ContractFriendlyName",  documentTitle },
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
