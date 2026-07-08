using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;

namespace Pds.Contracts.Notifications.Func.Contracts.Lifecycle
{
    /// <summary>
    /// Contracts ready to review notification service bus function.
    /// </summary>
    public class ContractReadyToReviewNotifierServiceBusFunction
    {
        private readonly IContractStatusChangePublisher _contractStatusChangePublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToReviewNotifierServiceBusFunction"/> class.
        /// </summary>
        /// <param name="contractStatusChangePublisher">An implementation of <see cref="IContractStatusChangePublisher"/>.</param>
        public ContractReadyToReviewNotifierServiceBusFunction(IContractStatusChangePublisher contractStatusChangePublisher)
        {
            _contractStatusChangePublisher = contractStatusChangePublisher;
        }

        /// <summary>
        /// Process contract change event for ready to review messages.
        /// </summary>
        /// <param name="contractChangeEvent">Contract change event.</param>
        /// <param name="log">for log output.</param>
        /// <returns>A task completetion from asynchronous operation.</returns>
        [Function(nameof(ContractReadyToReviewNotifierServiceBusFunction))]
        public async Task RunAsync([ServiceBusTrigger("%Pds.Contracts.Notifications.Topic%", "%Pds.Contracts.ReadyToReview.Subscription%", Connection = "AzureMessagingServiceBusOptions:ConnectionString")] Contract contractChangeEvent, ILogger log)
        {
            _ = contractChangeEvent ?? throw new ArgumentNullException(nameof(contractChangeEvent));

            log?.LogInformation($"{nameof(ContractReadyToReviewNotifierServiceBusFunction)} - ServiceBus topic triggered function for message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");

            await _contractStatusChangePublisher.NotifyContractChangesAreReadyForReviewAsync(contractChangeEvent);

            log?.LogInformation($"{nameof(ContractReadyToReviewNotifierServiceBusFunction)} - ServiceBus topic trigger function processed message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");
        }
    }
}