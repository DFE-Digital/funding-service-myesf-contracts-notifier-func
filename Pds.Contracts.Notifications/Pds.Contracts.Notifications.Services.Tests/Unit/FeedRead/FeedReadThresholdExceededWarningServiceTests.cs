using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Implementations.FeedReed;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using Pds.Core.Notification.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit.FeedReed
{
    [TestClass, TestCategory("Unit")]
    public class FeedReadThresholdExceededWarningServiceTests
    {
        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<FeedReadThresholdExceededWarningService>> _mockLogger;
        private IConfiguration _configurationMock;
        private IFeedReadThresholdExceededWarningService _feedReadThresholdExceededWarningService;

        public FeedReadThresholdExceededWarningServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);

            _mockLogger = new Mock<ILoggerAdapter<FeedReadThresholdExceededWarningService>>(MockBehavior.Strict);

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CdsUserExceptionEmail", "testemail" }
            }).Build();

            _feedReadThresholdExceededWarningService = new FeedReadThresholdExceededWarningService(
                _mockNotificationEmailQueueService.Object,
                _configurationMock,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task Process_WhenMessageReceived_SuccessfullySendsFeedReadExceptionEmptyPageEmail()
        {
            // Arrange
            var serviceBusMessage = new FeedReadThresholdExceededWarningMessage()
            {
                Start = new DateTime(2024, 01, 01),
                Now = new DateTime(2024, 01, 01),
                BookmarkId = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                LastPageUrl = "https://skillsfunding.service.gov.uk/"
            };

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, Constants.MessageType_FeedReadThresholdExceededWarningEmail);

            SetupServiceMock();

            // Act
            await _feedReadThresholdExceededWarningService.Process(serviceBusMessage);

            // Assert
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenUnableToReadEmailFromConfig_ThrowsException()
        {
            // Arrange
            SetupServiceMock();
            var serviceBusMessage = new FeedReadThresholdExceededWarningMessage()
            {
                Start = new DateTime(2024, 01, 01),
                Now = new DateTime(2024, 01, 01),
                BookmarkId = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                LastPageUrl = "https://skillsfunding.service.gov.uk/"
            };
            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { }).Build();
            _feedReadThresholdExceededWarningService = new FeedReadThresholdExceededWarningService(
                _mockNotificationEmailQueueService.Object,
                _configurationMock,
                _mockLogger.Object);

            var expectedErrorMessage = $"{nameof(FeedReadThresholdExceededWarningMessage)}-Bookmarkid [{serviceBusMessage.BookmarkId}],Unable to find the email address from configuration->cdsUserExceptionEmail.";

            // Act
            Func<Task> actual = async () => { await _feedReadThresholdExceededWarningService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be(expectedErrorMessage);
            _mockLogger.Verify(logger => logger.LogError(expectedErrorMessage), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenNotificationEmailQueueServiceReturnsError_ThrowsException()
        {
            // Arrange
            SetupServiceMock(true);
            var serviceBusMessage = new FeedReadThresholdExceededWarningMessage()
            {
                Start = new DateTime(2024, 01, 01),
                Now = new DateTime(2024, 01, 01),
                BookmarkId = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                LastPageUrl = "https://skillsfunding.service.gov.uk/"
            };

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, Constants.MessageType_FeedReadThresholdExceededWarningEmail);

            // Act
            Func<Task> actual = async () => { await _feedReadThresholdExceededWarningService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private NotificationMessage GetNotificationMessageMock(FeedReadThresholdExceededWarningMessage feedReadExceptionMessage, string messageType)
        {
            return new NotificationMessage()
            {
                EmailMessageType = messageType,
                RequestingService = Constants.RequestingService_FundingClaims,
                EmailAddresses = new[] { _configurationMock.GetValue<string>("CdsUserExceptionEmail") }.AsEnumerable(),
                EmailPersonalisation = new GovUkNotifyPersonalisation()
                {
                    Personalisation = new Dictionary<string, object>()
                    {
                        { "Start",  feedReadExceptionMessage.Start.DisplayFormat() },
                        { "TimeWarningRaised",  feedReadExceptionMessage.Now.DisplayFormat() },
                        { "Bookmark",  feedReadExceptionMessage.BookmarkId },
                        { "Url",  feedReadExceptionMessage.LastPageUrl }
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
