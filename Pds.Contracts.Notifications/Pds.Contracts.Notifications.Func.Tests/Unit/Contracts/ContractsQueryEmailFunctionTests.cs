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
    public class ContractsQueryEmailFunctionTests
    {
        [TestMethod]
        public async Task Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractsQueryEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractsQueryEmailMessage>()))
                .Returns(Task.CompletedTask)
                .Verifiable(Times.Once);

            var function = new ContractsQueryEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ContractsQueryEmailMessage()
                {
                    ContractId = 1,
                    ProviderEmailAddress = "testemail",
                    ProviderName = "testname",
                    ProviderUserName = "testusername",
                    QueryDetail = "testdetail",
                    QueryReason = "testreason"
                });
            };

            // Assert
            await act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public async Task Run_ThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractsQueryEmailService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.Process(It.IsAny<ContractsQueryEmailMessage>()))
                .ThrowsAsync(It.IsAny<Exception>())
                .Verifiable(Times.Once);

            var function = new ContractsQueryEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ContractsQueryEmailMessage()
                {
                    ContractId = 1,
                    ProviderEmailAddress = "testemail",
                    ProviderName = "testname",
                    ProviderUserName = "testusername",
                    QueryDetail = "testdetail",
                    QueryReason = "testreason"
                });
            };

            // Assert
            await act.Should().ThrowAsync<Exception>();
            mockService.Verify();
        }

        [DataTestMethod]
        [DataRow("", 1, true)]
        [DataRow(null, 2, true)]
        [DataRow("provider1", 0)]
        public async Task Run_WhenValidationFailure_ThrowsArgumentException(string providerName, int contractId, bool isArgumentNullException = false)
        {
            // Arrange
            var mockService = new Mock<IContractsQueryEmailService>(MockBehavior.Strict);

            var function = new ContractsQueryEmailFunction(mockService.Object);

            // Act
            Func<Task> act = async () =>
            {
                await function.Run(new ContractsQueryEmailMessage()
                {
                    ContractId = contractId,
                    ProviderEmailAddress = "testemail",
                    ProviderName = providerName,
                    ProviderUserName = "testusername",
                    QueryDetail = "testdetail",
                    QueryReason = "testreason"
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
