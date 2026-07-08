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
    public class FundingClaimWithdrawnEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IFundingClaimWithdrawnEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FundingClaimWithdrawnEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new FundingClaimWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimWithdrawnEmailMessage() { FundingClaimId = 1 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IFundingClaimWithdrawnEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FundingClaimWithdrawnEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new FundingClaimWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimWithdrawnEmailMessage() { FundingClaimId = 1 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException()
        {
            var mockService = new Mock<IFundingClaimWithdrawnEmailService>(MockBehavior.Strict);

            var function = new FundingClaimWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimWithdrawnEmailMessage()); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
