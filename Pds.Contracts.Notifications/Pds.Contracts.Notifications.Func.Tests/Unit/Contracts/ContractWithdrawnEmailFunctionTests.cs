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
    public class ContractWithdrawnEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractWithdrawnEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractWithdrawnEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ContractWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractWithdrawnEmailMessage() { ContractNumber = "contract1", VersionNumber = 1, Ukprn = 12345678 }); };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractWithdrawnEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractWithdrawnEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ContractWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractWithdrawnEmailMessage() { ContractNumber = "contract1", VersionNumber = 1, Ukprn = 12345678 }); };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow("", 1, 12345678, null)]
        [DataRow("contract1", 0, 12345678)]
        [DataRow("contract1", 1, 0)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(string contractNumber, int versionNumber, int ukprn, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IContractWithdrawnEmailService>(MockBehavior.Strict);

            var function = new ContractWithdrawnEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(new ContractWithdrawnEmailMessage() { ContractNumber = contractNumber, VersionNumber = versionNumber, Ukprn = ukprn }); };

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
