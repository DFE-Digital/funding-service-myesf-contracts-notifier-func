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
    public class ContractReminderEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ContractReminderEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IDfESignInPublicApi> _mockDfESignInPublicApi;
        private readonly Mock<IAuditService> _mockAuditService;

        private readonly IContractReminderEmailService _contractReminderEmailService;

        public ContractReminderEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ContractReminderEmailService>>(MockBehavior.Strict);
            _mockDfESignInPublicApi = new Mock<IDfESignInPublicApi>(MockBehavior.Strict);
            _mockAuditService = new Mock<IAuditService>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _contractReminderEmailService = new ContractReminderEmailService(
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
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            SetupServicesMock(null, serviceBusMessage.ContractId, false, true);

            // Act
            Func<Task> actual = async () => { await _contractReminderEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
        }

        [TestMethod]
        public async Task Process_WithInvalidContract_DoesNotSendContractReminderEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = ContractFundingType.Aebp,
                Status = ContractStatus.Replaced,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            var expectedInfo = string.Format(
                Constants.LogMessage,
                mockContract.ContractDisplayText,
                $"{nameof(ContractReminderEmailMessage)} processed and contract status is not published to provider, Status: [{mockContract.Status}].");

            // Act
            await _contractReminderEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockLogger.Verify(logger => logger.LogInformation(expectedInfo), Times.Once);
        }

        [TestMethod]
        public async Task Process_WithValidContractAndFailedDSIApiCall_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = ContractFundingType.Aebp,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() }, false, true);

            // Act
            Func<Task> actual = async () => { await _contractReminderEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<GetApiResultException>();

            _mockHttp.VerifyNoOutstandingRequest();
        }

        [TestMethod]
        public async Task Process_WithValidContractAndInvalidRoles_DoesNotSendContractReminderEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = ContractFundingType.Aebp,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Notfication
            };

            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            SetupDfESignInPublicApiMock(new[] { UserRole.DocumentExchangeUser.ToString() }, true);

            var expectedError = string.Format(
                Constants.LogMessage,
                mockContract.ContractDisplayText,
                $"{nameof(ContractReminderEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}].");

            // Act
            await _contractReminderEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == $"{nameof(ContractReminderEmailMessage)} processed and no users found with roles [ViewContractsAndAgreements, SignContractsAndAgreements] for organisation [{mockContract.Ukprn}].")), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, "contract")]
        [DataRow(ContractFundingType.Levy, "agreement")]
        public async Task Process_WithValidContractAndRequiredUserRoles_SuccessfullySendsContractReminderEmail(ContractFundingType fundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = fundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReminderEmail, documentType);

            var expectedAuditMessage = "ContractReminderEmailMessage processed and published to SharedEmailprocessorQueue.";


            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            await _contractReminderEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockDfESignInPublicApi.Verify(api => api.GetUserContactsForOrganisation(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
            _mockAuditService.Verify(audit => audit.AuditAsync(It.Is<Audit.Api.Client.Models.Audit>(x => x.Message == expectedAuditMessage)), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ContractFundingType.Aebp, "contract")]
        [DataRow(ContractFundingType.Levy, "agreement")]
        public async Task Process_WithValidContractRequiredUserRolesAndNotificationServiceReturnsError_ThrowsException(ContractFundingType fundingType, string documentType)
        {
            // Arrange
            var serviceBusMessage = new ContractReminderEmailMessage()
            {
                ContractId = 1
            };

            var mockContract = new Contract
            {
                Title = "mockTitle",
                FundingType = fundingType,
                Status = ContractStatus.PublishedToProvider,
                AmendmentType = ContractAmendmentType.Notfication
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractReminderEmail, documentType);

            SetupServicesMock(mockContract, serviceBusMessage.ContractId, true);

            SetupDfESignInPublicApiMock(new[] { UserRole.SignContractsAndAgreements.ToString(), UserRole.ViewContractsAndAgreements.ToString() });

            // Act
            Func<Task> actual = async () => { await _contractReminderEmailService.Process(serviceBusMessage); };

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

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, string messageType, string documentType)
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

        private void SetupServicesMock(Contract mockContract, int contractId, bool isNotificationEmailQueueServiceThrowException = false, bool isContractDataApiThrowException = false)
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
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractGetByIdEndpoint + "/" + contractId)
                .Throw(new ApiGeneralException());
            }
            else
            {
                _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + Constants.ContractGetByIdEndpoint + "/" + contractId)
                .Respond(HttpStatusCode.OK, response);
            }

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
