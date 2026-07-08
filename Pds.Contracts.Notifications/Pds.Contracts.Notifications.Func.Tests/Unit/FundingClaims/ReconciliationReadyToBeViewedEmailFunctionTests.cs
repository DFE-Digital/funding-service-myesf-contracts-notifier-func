using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.FundingClaims;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.FundingClaims
{
    [TestClass, TestCategory("Unit")]
    public class ReconciliationReadyToBeViewedEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IReconciliationReadyToBeViewedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ReconciliationReadyToBeViewedEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ReconciliationReadyToBeViewedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ReconciliationReadyToBeViewedEmailMessage() { ReconciliationId = 1 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IReconciliationReadyToBeViewedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ReconciliationReadyToBeViewedEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ReconciliationReadyToBeViewedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ReconciliationReadyToBeViewedEmailMessage() { ReconciliationId = 1 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException()
        {
            var mockService = new Mock<IReconciliationReadyToBeViewedEmailService>(MockBehavior.Strict);

            var function = new ReconciliationReadyToBeViewedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ReconciliationReadyToBeViewedEmailMessage()); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
