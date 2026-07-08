using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.FeedReed;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.FeedRead
{
    [TestClass, TestCategory("Unit")]
    public class FeedReadThresholdExceededWarningFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IFeedReadThresholdExceededWarningService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FeedReadThresholdExceededWarningMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new FeedReadThresholdExceededWarningFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FeedReadThresholdExceededWarningMessage() { Start = DateTime.UtcNow, Now = DateTime.UtcNow, BookmarkId = Guid.NewGuid(), LastPageUrl = "url" }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IFeedReadThresholdExceededWarningService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FeedReadThresholdExceededWarningMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new FeedReadThresholdExceededWarningFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FeedReadThresholdExceededWarningMessage() { Start = DateTime.UtcNow, Now = DateTime.UtcNow, BookmarkId = Guid.NewGuid(), LastPageUrl = "url" }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow(false, "def6ea78-ee68-43d3-939a-5ec04eb822d6", "url")]
        [DataRow(true, null, "url")]
        [DataRow(true, "def6ea78-ee68-43d3-939a-5ec04eb822d6", "", true)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(bool isStartDateAvailable, string bookmarkId, string lastPageUrl, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IFeedReadThresholdExceededWarningService>(MockBehavior.Strict);

            var function = new FeedReadThresholdExceededWarningFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new FeedReadThresholdExceededWarningMessage()
                {
                    Start = isStartDateAvailable ? DateTime.UtcNow : default,
                    Now = DateTime.UtcNow,
                    BookmarkId = Guid.Parse(bookmarkId),
                    LastPageUrl = lastPageUrl
                });
            };

            // Assert
            if (isArgumentNullException)
            {
                await act.Should().ThrowAsync<ArgumentNullException>();
            }
            else
            {
                await act.Should().ThrowAsync<ArgumentException>();
            }
        }
    }
}
