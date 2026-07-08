using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Func.Contracts.Lifecycle;
using Pds.Contracts.Notifications.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit.Contracts.Lifecycle
{
    [TestClass, TestCategory("Unit")]
    public class ContractsReminderTimerFunctionTests
    {
        [TestMethod]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractReminderProcessingService>(MockBehavior.Strict);

            mockService
                .Setup(e => e.IssueContractReminders())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var function = new ContractsReminderTimerFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(null, null); };

            // Assert
            act.Should().NotThrowAsync();
            mockService.Verify();
        }
    }
}