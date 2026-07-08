using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.Contracts.Lifecycle;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.Contracts.Lifecycle
{
    [TestClass, TestCategory("Unit")]
    public class ContractApprovedNotifierServiceBusFunctionTests
    {
        [TestMethod]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractStatusChangePublisher>(MockBehavior.Strict);

            mockService
                .Setup(e => e.NotifyContractApprovedAsync(It.IsAny<Contract>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var function = new ContractApprovedNotifierServiceBusFunction(mockService.Object);

            var jsonContract = """
                {
                  "Id": 1,
                  "ContractNumber": "APPLEVY-1234",
                  "ContractVersion": 1,
                  "Status": "PublishedToProvider",
                  "UKPRN": 12345678
                }
                """;
            var contract = JsonSerializer.Deserialize<Contract>(jsonContract);

            // Act
            Func<Task> act = async () => { await function.RunAsync(contract, null); };

            // Assert
            act.Should().NotThrowAsync();
            mockService.Verify();
        }

        [TestMethod]
        public void Run_ThrowsArgumentNullException()
        {
            // Arrange
            var mockService = new Mock<IContractStatusChangePublisher>(MockBehavior.Strict);
            var function = new ContractApprovedNotifierServiceBusFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.RunAsync(null, null); };

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
            mockService.VerifyNoOtherCalls();
        }
    }
}