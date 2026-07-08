using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using Pds.Core.Azure.Interfaces.Messaging;
using System;
using System.Threading.Tasks;
using ConfigurationConstants = Pds.Contracts.Notifications.Services.Configuration;
using SFS = Sfa.Sfs.Contracts.Messaging;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractStatusChangePublisherTests
    {
        private readonly Contract _dummyContract;
        private readonly IAzureMessagingServiceBusService _mockAzureMessagingServiceBusService;
        private readonly IAuditService _mockAuditService;
        private readonly ILogger<IContractStatusChangePublisher> _mockLogger;

        public ContractStatusChangePublisherTests()
        {
            _dummyContract = new Models.Contract { Id = 1, ContractNumber = "Test", ContractVersion = 1, Ukprn = 123456 };
            _mockAzureMessagingServiceBusService = Mock.Of<IAzureMessagingServiceBusService>(MockBehavior.Strict);
            _mockAuditService = Mock.Of<IAuditService>(MockBehavior.Strict);
            _mockLogger = Mock.Of<ILogger<Interfaces.IContractStatusChangePublisher>>(MockBehavior.Strict);

            Mock.Get(_mockLogger)
                .Setup(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [TestMethod]
        public async Task NotifyContractApprovedTestAsync()
        {
            // Arrange
            SetupNotifyAsyncPrivateMethodCalls(ConfigurationConstants.Constants.ContractApprovedEmailQueue);
            _dummyContract.Status = ContractStatus.Approved;
            var expectedMessage = new SFS.ContractApprovedMessage { ContractId = _dummyContract.Id };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractApprovedAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockAzureMessagingServiceBusService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractApprovedAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage, ConfigurationConstants.Constants.ContractApprovedEmailQueue);
        }

        [TestMethod]
        public async Task NotifyContractChangesAreReadyForReviewTestAsync()
        {
            // Arrange
            SetupNotifyAsyncPrivateMethodCalls(ConfigurationConstants.Constants.ContractReadyToReviewEmailQueue);
            _dummyContract.Status = ContractStatus.Approved;
            var expectedMessage = new SFS.ContractReadyToReviewEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractChangesAreReadyForReviewAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockAzureMessagingServiceBusService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractChangesAreReadyForReviewAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage, ConfigurationConstants.Constants.ContractReadyToReviewEmailQueue);
        }

        [TestMethod]
        public async Task NotifyContractIsReadyToSignTestAsync()
        {
            // Arrange
            SetupNotifyAsyncPrivateMethodCalls(ConfigurationConstants.Constants.ContractReadyToSignEmailQueue);
            _dummyContract.Status = ContractStatus.PublishedToProvider;
            var expectedMessage = new SFS.ContractReadyToSignEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractIsReadyToSignAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockAzureMessagingServiceBusService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractIsReadyToSignAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage, ConfigurationConstants.Constants.ContractReadyToSignEmailQueue);
        }

        [DataTestMethod]
        [DataRow(ContractStatus.WithdrawnByAgency)]
        [DataRow(ContractStatus.WithdrawnByProvider)]
        public async Task NotifyContractWithdrawnTestAsync(ContractStatus withdrawnStatus)
        {
            // Arrange
            SetupNotifyAsyncPrivateMethodCalls(ConfigurationConstants.Constants.ContractWithdrawnEmailQueue);
            _dummyContract.Status = withdrawnStatus;
            var expectedMessage = new SFS.ContractWithdrawnEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractWithdrawnAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{withdrawnStatus}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockAzureMessagingServiceBusService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractWithdrawnAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage, ConfigurationConstants.Constants.ContractWithdrawnEmailQueue);
        }

        private void SetupNotifyAsyncPrivateMethodCalls(string queueName)
        {
            Mock.Get(_mockAzureMessagingServiceBusService)
                .Setup(sbs => sbs.SendMessageAsync(It.IsAny<It.IsAnyType>(), queueName, null))
                .Returns(Task.CompletedTask);

            Mock.Get(_mockAuditService)
                .Setup(a => a.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask);
        }

        private void VerifyAllMocks<T>(T message, Audit.Api.Client.Models.Audit auditMessage, string queueName)
        {
            Mock.Get(_mockLogger).VerifyAll();
            Mock.Get(_mockAzureMessagingServiceBusService)
                .Verify(sbs => sbs.SendMessageAsync(ItIs.EquivalentTo(message), ItIs.EquivalentTo(queueName), null), Times.Once);

            Mock.Get(_mockAuditService)
                .Verify(a => a.AuditAsync(ItIs.EquivalentTo(auditMessage)), Times.Once);
        }
    }
}