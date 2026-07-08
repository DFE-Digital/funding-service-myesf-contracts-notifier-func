using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations.FeedReed;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models;
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
    public class FeedReadExceptionServiceTests
    {
        private readonly Mock<INotificationEmailQueueService> _mockNotificationEmailQueueService;
        private readonly Mock<ILoggerAdapter<FeedReadExceptionService>> _mocklogger;
        private IConfiguration _configurationMock;
        private IFeedReadExceptionService _feedReadExceptionService;

        public FeedReadExceptionServiceTests()
        {
            _mockNotificationEmailQueueService = new Mock<INotificationEmailQueueService>(MockBehavior.Strict);
            _mocklogger = new Mock<ILoggerAdapter<FeedReadExceptionService>>(MockBehavior.Strict);

            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CdsUserExceptionEmail", "testemail" }
            }).Build();

            _feedReadExceptionService = new FeedReadExceptionService(_mockNotificationEmailQueueService.Object, _configurationMock, _mocklogger.Object);
        }

        [DataTestMethod]
        [DataRow(ExceptionType.EmptyPageOnFeed, Constants.MessageType_FeedReadExceptionEmptyPageEmail)]
        [DataRow(ExceptionType.BookmarkNotMatched, Constants.MessageType_FeedReadExceptionBookmarkNotMatchedEmail)]
        public async Task Process_WhenMessageReceived_SuccessfullySendsFeedReadExceptionEmptyPageEmail(ExceptionType exceptionType, string messageType)
        {
            // Arrange
            var serviceBusMessage = new FeedReadExceptionMessage()
            {
                Type = exceptionType,
                Bookmark = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                Url = "https://skillsfunding.service.gov.uk/"
            };

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, messageType);

            SetupServiceMock();

            // Act
            await _feedReadExceptionService.Process(serviceBusMessage);

            // Assert
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        [TestMethod]
        public async Task Process_WhenIncorrectMessageTypeReceived_ThrowsException()
        {
            //Arrange
            SetupServiceMock();
            var serviceBusMessage = new FeedReadExceptionMessage()
            {
                Type = (ExceptionType)3,
                Bookmark = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                Url = "https://skillsfunding.service.gov.uk/"
            };

            // Act
            Func<Task> actual = async () => { await _feedReadExceptionService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<ArgumentOutOfRangeException>();
            result.Which.Message.Should().Contain($"No email template found for expection type [{serviceBusMessage.Type.ToString()}]");
        }

        [DataTestMethod]
        [DataRow(ExceptionType.EmptyPageOnFeed)]
        [DataRow(ExceptionType.BookmarkNotMatched)]
        public async Task Process_WhenUnableToReadEmailFromConfig_ThrowsException(ExceptionType exceptionType)
        {
            // Arrange
            SetupServiceMock();
            var serviceBusMessage = new FeedReadExceptionMessage()
            {
                Type = exceptionType,
                Bookmark = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                Url = "https://skillsfunding.service.gov.uk/"
            };
            _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { }).Build();
            _feedReadExceptionService = new FeedReadExceptionService(_mockNotificationEmailQueueService.Object, _configurationMock, _mocklogger.Object);

            var expectedErrorMessage = $"{nameof(FeedReadExceptionMessage)}-[{serviceBusMessage.Type}]-[{serviceBusMessage.Bookmark}], Unable to find the email address from configuration->cdsUserExceptionEmail.";

            // Act
            Func<Task> actual = async () => { await _feedReadExceptionService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            result.Which.Message.Should().Be(expectedErrorMessage);
            _mocklogger.Verify(logger => logger.LogError(expectedErrorMessage), Times.Once);
        }

        [DataTestMethod]
        [DataRow(ExceptionType.EmptyPageOnFeed, Constants.MessageType_FeedReadExceptionEmptyPageEmail)]
        [DataRow(ExceptionType.BookmarkNotMatched, Constants.MessageType_FeedReadExceptionBookmarkNotMatchedEmail)]
        public async Task Process_WhenNotificationEmailQueueServiceReturnsError_ThrowsException(ExceptionType exceptionType, string messageType)
        {
            // Arrange
            SetupServiceMock(true);
            var serviceBusMessage = new FeedReadExceptionMessage()
            {
                Type = exceptionType,
                Bookmark = new Guid("a85a4b6b-2f63-4934-9b74-7477e40aad81"),
                Url = "https://skillsfunding.service.gov.uk/"
            };

            var expectedMessage = GetNotificationMessageMock(serviceBusMessage, messageType);

            // Act
            Func<Task> actual = async () => { await _feedReadExceptionService.Process(serviceBusMessage); };

            // Assert
            var result = await actual.Should().ThrowAsync<Exception>();
            _mockNotificationEmailQueueService.Verify(service => service.SendAsync(ItIs.EquivalentTo(expectedMessage)), Times.Once);
        }

        private NotificationMessage GetNotificationMessageMock(FeedReadExceptionMessage feedReadExceptionMessage, string messageType)
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
                         { "Bookmark",  feedReadExceptionMessage.Bookmark },
                         { "Url",  feedReadExceptionMessage.Url }
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

            _mocklogger.Setup(logger => logger.LogInformation(It.IsAny<string>()));
            _mocklogger.Setup(logger => logger.LogError(It.IsAny<string>()));
        }
    }
}
