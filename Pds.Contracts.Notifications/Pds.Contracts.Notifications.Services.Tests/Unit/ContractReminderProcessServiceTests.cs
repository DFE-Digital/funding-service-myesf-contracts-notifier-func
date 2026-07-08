using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractReminderProcessServiceTests
    {
        private readonly IContractNotificationService _contractNotificationService
            = Mock.Of<IContractNotificationService>(MockBehavior.Strict);

        private readonly ILoggerAdapter<ContractReminderProcessingService> _loggerAdapter
            = Mock.Of<ILoggerAdapter<ContractReminderProcessingService>>(MockBehavior.Strict);

        private readonly IAuditService _auditService
            = Mock.Of<IAuditService>(MockBehavior.Strict);

        [TestMethod]
        public async Task IssueContactReminders_HandlesNullResult()
        {
            // Arrange
            var service = CreateContractReminderService();

            ContractReminders result = null;
            SetupContractReminderServiceGetContracts(result, null);

            SetupLoggerInformation();

            Mock.Get(_auditService)
                .Setup(p => p.TrySendAuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            Func<Task> act = async () => await service.IssueContractReminders();

            // Assert
            await act.Should().NotThrowAsync();
            VerifyAll(result, 1, 0, 0);
        }

        [TestMethod]
        public async Task IssueContactReminders_ProcessesAllReturnedRecords()
        {
            // Arrange
            var service = CreateContractReminderService();

            var result = GetContracts();
            var emptyResult = new ContractReminders();

            SetupContractReminderServiceGetContracts(result, emptyResult);

            SetupLoggerInformation();

            Mock.Get(_contractNotificationService)
                .Setup(p => p.QueueContractEmailReminderMessage(It.IsIn<Contract>(result.Contracts)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_contractNotificationService)
                .Setup(p => p.NotifyContractReminderSent(It.IsIn<Contract>(result.Contracts)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.TrySendAuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            Func<Task> act = async () => await service.IssueContractReminders();

            // Assert
            await act.Should().NotThrowAsync();
            VerifyAll(result, 2, 2, 2);
        }

        [TestMethod]
        public async Task IssueContactReminders_OnOneRecordProcessingFailure_LogsErrors_And_Continues()
        {
            // Arrange
            var service = new ContractReminderProcessingService(_contractNotificationService, _loggerAdapter, _auditService);

            var result = GetContracts();
            var emptyResult = new ContractReminders();

            SetupContractReminderServiceGetContracts(result, emptyResult);
            SetupLoggerInformation();
            SetupLoggerError<InvalidOperationException>();

            Mock.Get(_contractNotificationService)
                .SetupSequence(p => p.QueueContractEmailReminderMessage(It.IsIn<Contract>(result.Contracts)))
                .Returns(Task.CompletedTask)
                .Throws<InvalidOperationException>()
                .Returns(Task.CompletedTask);

            Mock.Get(_contractNotificationService)
                .Setup(p => p.NotifyContractReminderSent(It.IsIn<Contract>(result.Contracts)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.TrySendAuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            Func<Task> act = async () => await service.IssueContractReminders();

            // Assert
            await act.Should().NotThrowAsync();
            Mock.Get(_contractNotificationService).Verify();
            Mock.Get(_loggerAdapter).Verify();
        }

        [TestMethod]
        public async Task IssueContactReminders_OnAllProcessingFailure_RaisesException()
        {
            // Arrange
            var service = CreateContractReminderService();
            var result = GetContracts();
            var emptyResult = new ContractReminders();

            SetupContractReminderServiceGetContracts(result, emptyResult);

            SetupLoggerInformation();
            SetupLoggerError<InvalidOperationException>();

            Mock.Get(_loggerAdapter)
                .Setup(p => p.LogCritical(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            Mock.Get(_contractNotificationService)
                .Setup(p => p.QueueContractEmailReminderMessage(It.IsIn<Contract>(result.Contracts)))
                .Throws<InvalidOperationException>()
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.TrySendAuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            Func<Task> act = async () => await service.IssueContractReminders();

            // Assert
            await act.Should().ThrowAsync<Exception>();
            Mock.Get(_contractNotificationService).Verify();
            Mock.Get(_loggerAdapter).Verify();
        }

        #region Setup Helpers

        private ContractReminderProcessingService CreateContractReminderService()
            => new ContractReminderProcessingService(_contractNotificationService, _loggerAdapter, _auditService);

        private void SetupLoggerError<TException>()
            where TException : Exception
        {
            Mock.Get(_loggerAdapter)
                .Setup(p => p.LogError(It.IsAny<TException>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void SetupLoggerInformation()
        {
            Mock.Get(_loggerAdapter)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void SetupContractReminderServiceGetContracts(ContractReminders firstResult, ContractReminders secondResult)
        {
            Mock.Get(_contractNotificationService)
                .SetupSequence(p => p.GetOverdueContracts())
                .Returns(Task.FromResult(firstResult))
                .Returns(Task.FromResult(secondResult))
                .Throws<InvalidOperationException>();
        }

        private ContractReminders GetContracts()
        {
            return new ContractReminders()
            {
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        ContractNumber = "123",
                        ContractVersion = 234,
                        Id = 345,
                        Ukprn = 456
                    },
                    new Contract()
                    {
                        ContractNumber = "567",
                        ContractVersion = 678,
                        Id = 789,
                        Ukprn = 891
                    }
                }
            };
        }

        #endregion


        #region Verify Helpers

        private void VerifyAll(ContractReminders result, int callsToGetOverdue, int callsToEmailReminder, int callsToReminderSent)
        {
            var contracts = result?.Contracts;

            Mock.Get(_contractNotificationService)
                .Verify(p => p.GetOverdueContracts(), Times.Exactly(callsToGetOverdue));
            Mock.Get(_contractNotificationService)
                .Verify(p => p.QueueContractEmailReminderMessage(It.IsIn<Contract>(contracts)), Times.Exactly(callsToEmailReminder));
            Mock.Get(_contractNotificationService)
                .Verify(p => p.NotifyContractReminderSent(It.IsIn<Contract>(contracts)), Times.Exactly(callsToReminderSent));
            Mock.Get(_loggerAdapter).VerifyAll();
        }

        #endregion
    }
}
