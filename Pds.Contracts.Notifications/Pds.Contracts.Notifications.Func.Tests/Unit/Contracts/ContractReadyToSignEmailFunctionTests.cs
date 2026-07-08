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
    public class ContractReadyToSignEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractReadyToSignEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractReadyToSignEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ContractReadyToSignEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractReadyToSignEmailMessage() { ContractNumber = "contract1", VersionNumber = 1, Ukprn = 12345678 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractReadyToSignEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractReadyToSignEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ContractReadyToSignEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractReadyToSignEmailMessage() { ContractNumber = "contract1", VersionNumber = 1, Ukprn = 12345678 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow("", 1, 12345678, true)]
        [DataRow("contract1", 0, 12345678)]
        [DataRow("contract1", 1, 0)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(string contractNumber, int versionNumber, int ukprn, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IContractReadyToSignEmailService>(MockBehavior.Strict);

            var function = new ContractReadyToSignEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractReadyToSignEmailMessage() { ContractNumber = contractNumber, VersionNumber = versionNumber, Ukprn = ukprn }); };

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
