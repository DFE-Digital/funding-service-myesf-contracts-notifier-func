using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Implementations.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using Pds.Core.Notification.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.Contracts
{
    [TestClass, TestCategory("Unit")]
    public class ProcessContractFromFeedExceptionServiceTests
    {
        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<ProcessContractFromFeedExceptionService>> _mockLogger;
        private IConfiguration _configurationMock;
        private IProcessContractFromFeedExceptionService _processContractFromFeedExceptionService;

        public ProcessContractFromFeedExceptionServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILoggerAdapter<ProcessContractFromFeedExceptionService>>(MockBehavior.Strict);

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CdsUserExceptionEmail", "testemail" }
            }).Build();

            _processContractFromFeedExceptionService = new ProcessContractFromFeedExceptionService(
                _mockNotificationEmailQueueService.Object,
                _mockLogger.Object,
                _configurationMock);
        }

        [TestMethod]
        public async Task Process_WhenMessageReceived_SuccessfullySendsProcessContractFromFeedExceptionEmail()
        {
            // Arrange
            var serviceBusMessage = MockProcessContractFromFeedExceptionMessage();

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, Constants.MessageType_ProcessContractFromFeedException);

            SetupServiceMock();

            // Act
            await _processContractFromFeedExceptionService.Process(serviceBusMessage);

            // Assert
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenUnableToReadEmailFromConfig_ThrowsException()
        {
            // Arrange
            SetupServiceMock();
            var serviceBusMessage = MockProcessContractFromFeedExceptionMessage();

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { }).Build();
            _processContractFromFeedExceptionService = new ProcessContractFromFeedExceptionService(_mockNotificationEmailQueueService.Object, _mockLogger.Object, _configurationMock);

            var expectedError = string.Format(
                Constants.LogMessage,
                $"{serviceBusMessage.ContractNumber}-{serviceBusMessage.ContractVersionNumber}",
                "Unable to find the email address from configuration->cdsUserExceptionEmail.");

            // Act
            Func<Task> actual = async () => { await _processContractFromFeedExceptionService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be(expectedError);
            _mockLogger.Verify(logger => logger.LogError(expectedError), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenNotificationEmailQueueServiceReturnsError_ThrowsException()
        {
            // Arrange
            SetupServiceMock(true);
            var serviceBusMessage = MockProcessContractFromFeedExceptionMessage();

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, Constants.MessageType_ProcessContractFromFeedException);

            // Act
            Func<Task> actual = async () => { await _processContractFromFeedExceptionService.Process(serviceBusMessage); };

            // Assert
            await actual.Should().ThrowAsync<Exception>();
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private static ProcessContractFromFeedExceptionMessage MockProcessContractFromFeedExceptionMessage()
        {
            return new ProcessContractFromFeedExceptionMessage()
            {
                ParentFeedStatus = "testParentFeedStatus",
                FeedStatus = "testFeedStatus",
                ExistingContractStatus = "testExistingContractStatus",
                ParentContractNumber = "testParentContractNumber",
                ContractNumber = "testContractNumber",
                ContractVersionNumber = 1,
                ContractTitle = "testContractTitle",
                ExceptionTime = DateTime.UtcNow,
                ProviderName = "testProviderName",
                Ukprn = 12345678
            };
        }

        private NotificationMessage GetNotificationMessageMock(ProcessContractFromFeedExceptionMessage processContractFromFeedExceptionMessage, string messageType)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_Contracts,
                EmailAddresses = new[] { _configurationMock.GetValue<string>("CdsUserExceptionEmail") }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "FeedStatus",  processContractFromFeedExceptionMessage.FeedStatus },
                        { "ExistingContractStatus",  processContractFromFeedExceptionMessage.ExistingContractStatus },
                        { "ContractNumber",  processContractFromFeedExceptionMessage.ContractNumber },
                        { "ParentContractNumber",  processContractFromFeedExceptionMessage.ParentContractNumber },
                        { "ContractVersionNumber",  processContractFromFeedExceptionMessage.ContractVersionNumber },
                        { "ContractTitle",  processContractFromFeedExceptionMessage.ContractTitle },
                        { "ParentFeedStatus",  processContractFromFeedExceptionMessage.ParentFeedStatus },
                        { "ExceptionTime",  processContractFromFeedExceptionMessage.ExceptionTime.DisplayFormat() },
                        { "ProviderName",  processContractFromFeedExceptionMessage.ProviderName },
                        { "Ukprn",  processContractFromFeedExceptionMessage.Ukprn }
                    }
                }
            };
        }

        private void SetupServiceMock(bool isNotificationEmailQueueServiceThrowException = false)
        {
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

            _mockLogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
