using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.FeedReed;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.FeedRead
{
    [TestClass, TestCategory("Unit")]
    public class FeedReadExceptionFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IFeedReadExceptionService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FeedReadExceptionMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new FeedReadExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FeedReadExceptionMessage() { Type = ExceptionType.EmptyPageOnFeed, Bookmark = Guid.NewGuid(), Url = "testBookmark" }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IFeedReadExceptionService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FeedReadExceptionMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new FeedReadExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FeedReadExceptionMessage() { Type = ExceptionType.EmptyPageOnFeed, Bookmark = Guid.NewGuid(), Url = "testBookmark" }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow(3, "def6ea78-ee68-43d3-939a-5ec04eb822d6", "url")]
        [DataRow(0, null, "url")]
        [DataRow(1, "def6ea78-ee68-43d3-939a-5ec04eb822d6", "", true)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(ExceptionType type, string bookmark, string url, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IFeedReadExceptionService>(MockBehavior.Strict);

            var function = new FeedReadExceptionFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FeedReadExceptionMessage() { Type = type, Bookmark = Guid.Parse(bookmark), Url = url }); };

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
