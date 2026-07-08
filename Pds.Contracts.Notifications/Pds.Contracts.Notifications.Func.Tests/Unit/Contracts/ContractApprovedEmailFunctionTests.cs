using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.Contracts
{
    [TestClass, TestCategory("Unit")]
    public class ContractApprovedEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractApprovedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractApprovedEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ContractApprovedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => await function.Run(new ContractApprovedEmailMessage() { ContractId = 1 });

            // Assert
            await act.Should().NotThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractApprovedEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractApprovedEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ContractApprovedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractApprovedEmailMessage() { ContractId = 1 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException()
        {
            // Arrange
            var mockService = new Mock<IContractApprovedEmailService>(MockBehavior.Strict);

            var function = new ContractApprovedEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => await function.Run(new ContractApprovedEmailMessage());

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
