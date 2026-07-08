using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
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
    public class ContractsQueryEmailServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";
        private const string TestFakeAccessToken = "AccessToken";

        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ContractsQueryEmailService>> _mockLogger;
        private readonly MockHttpMessageHandler _mockHttp;
        private IContractsQueryEmailService _contractsQueryEmailService;
        private IConfiguration _configurationMock;

        public ContractsQueryEmailServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ContractsQueryEmailService>>(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ServiceNowEmailAddress", "testemail" }
            }).Build();

            _contractsQueryEmailService = new ContractsQueryEmailService(
                CreateAuthenticationService(),
                Options.Create(CreateContractsDataApiConfiguration()),
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _configurationMock,
                _mockHttp.ToHttpClient());
        }

        [TestMethod]
        public async Task Process_WhenContractDataApiUnreachable_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractsQueryEmailMessage()
            {
                ProviderUserName = "testUserName",
                ProviderName = "testName",
                ProviderEmailAddress = "testEmailAddress",
                ContractId = 3739,
                QueryReason = "test reason",
                QueryDetail = "test detail"
            };

            SetupServicesMock(null, serviceBusMessage.ContractId, false, true);

            // Act
            Func<Task> actual = async () => { await _contractsQueryEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<ApiGeneralException>();
        }

        [TestMethod]
        public async Task Process_WhenUnableToReadEmailFromConfig_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractsQueryEmailMessage()
            {
                ProviderUserName = "testUserName",
                ProviderName = "testName",
                ProviderEmailAddress = "testEmailAddress",
                ContractId = 3739,
                QueryReason = "test reason",
                QueryDetail = "test detail"
            };

            var mockContract = new Contract
            {
                Ukprn = 12345678,
                Title = "mockTitle",
                ContractContent = new ContractContent
                {
                    FileName = "testFileName"
                }
            };

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { }).Build();
            _contractsQueryEmailService = new ContractsQueryEmailService(
                CreateAuthenticationService(),
                Options.Create(CreateContractsDataApiConfiguration()),
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _configurationMock,
                _mockHttp.ToHttpClient());

            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            var expectedError = string.Format(
                Constants.LogMessage,
                mockContract.ContractDisplayText,
                "Unable to find the email address from configuration->ServiceNowEmailAddress.");

            // Assert
            var result = await Assert.ThrowsExceptionAsync<Exception>(async () => await _contractsQueryEmailService.Process(serviceBusMessage));
            Assert.AreEqual(expectedError, result.Message);

            _mockHttp.VerifyNoOutstandingRequest();
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenNotificationEmailQueueServiceReturnsError_ThrowsException()
        {
            // Arrange
            var serviceBusMessage = new ContractsQueryEmailMessage()
            {
                ProviderUserName = "testUserName",
                ProviderName = "testName",
                ProviderEmailAddress = "testEmailAddress",
                ContractId = 3739,
                QueryReason = "test reason",
                QueryDetail = "test detail"
            };

            var mockContract = new Contract
            {
                Ukprn = 12345678,
                Title = "mockTitle",
                ContractContent = new ContractContent
                {
                    FileName = "testFileName"
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractsQueryEmail);

            SetupServicesMock(mockContract, serviceBusMessage.ContractId, true);

            // Act
            Func<Task> actual = async () => { await _contractsQueryEmailService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();
            _mockHttp.VerifyNoOutstandingRequest();
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenMessageReceived_SuccessfullySendsContractsQueryEmail()
        {
            // Arrange
            var serviceBusMessage = new ContractsQueryEmailMessage()
            {
                ProviderUserName = "testUserName",
                ProviderName = "testName",
                ProviderEmailAddress = "testEmailAddress",
                ContractId = 3739,
                QueryReason = "test reason",
                QueryDetail = "test detail"
            };

            var mockContract = new Contract
            {
                Ukprn = 12345678,
                Title = "mockTitle",
                ContractContent = new ContractContent
                {
                    FileName = "testFileName"
                }
            };

            var expectedMessage = GetNotificationMessageMock(mockContract.Title, Constants.MessageType_ContractsQueryEmail);

            SetupServicesMock(mockContract, serviceBusMessage.ContractId);

            // Act
            await _contractsQueryEmailService.Process(serviceBusMessage);

            // Assert
            _mockHttp.VerifyNoOutstandingRequest();
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static ContractsDataApiConfiguration CreateContractsDataApiConfiguration()
        => new ()
        {
            ApiBaseAddress = TestBaseAddress
        };

        private static NotificationMessage GetNotificationMessageMock(string documentTitle, string messageType)
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
                        { "ProviderUserName", "testUserName" },
                        { "ProviderEmailAddress", "testEmailAddress" },
                        { "ProviderName", "testName" },
                        { "ProviderUkprn",  12345678 },
                        { "ContractTitle",  documentTitle },
                        { "ContractDocumentName",  "testFileName" },
                        { "QueryReason",  "test reason" },
                        { "QueryDetail",  "test detail" }
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
