using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.FundingClaims;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.FundingClaims.Notifications.Func.Tests.Unit.FundingClaims
{
    [TestClass, TestCategory("Unit")]
    public class FundingClaimSignedEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IFundingClaimSignedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FundingClaimSignedEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new FundingClaimSignedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimSignedEmailMessage() { FundingClaimId = 1 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IFundingClaimSignedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<FundingClaimSignedEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new FundingClaimSignedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimSignedEmailMessage() { FundingClaimId = 1 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException()
        {
            var mockService = new Mock<IFundingClaimSignedEmailService>(MockBehavior.Strict);

            var function = new FundingClaimSignedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new FundingClaimSignedEmailMessage()); };

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
