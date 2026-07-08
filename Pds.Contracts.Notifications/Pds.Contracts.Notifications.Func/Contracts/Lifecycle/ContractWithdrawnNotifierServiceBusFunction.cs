using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;

namespace Pds.Contracts.Notifications.Func.Contracts.Lifecycle
{
    /// <summary>
    /// Contracts withdrawn notification service bus function.
    /// </summary>
    public class ContractWithdrawnNotifierServiceBusFunction
    {
        private readonly IContractStatusChangePublisher _contractStatusChangePublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractWithdrawnNotifierServiceBusFunction"/> class.
        /// </summary>
        /// <param name="contractStatusChangePublisher">An implementation of <see cref="IContractStatusChangePublisher"/>.</param>
        public ContractWithdrawnNotifierServiceBusFunction(IContractStatusChangePublisher contractStatusChangePublisher)
        {
            _contractStatusChangePublisher = contractStatusChangePublisher;
        }

        /// <summary>
        /// Process contract change event for withdrawn messages.
        /// </summary>
        /// <param name="contractChangeEvent">Contract change event.</param>
        /// <param name="log">for log output.</param>
        /// <returns>A task completetion from asynchronous operation.</returns>
        [Function(nameof(ContractWithdrawnNotifierServiceBusFunction))]
        public async Task RunAsync([ServiceBusTrigger("%Pds.Contracts.Notifications.Topic%", "%Pds.Contracts.Withdrawn.Subscription%", Connection = "AzureMessagingServiceBusOptions:ConnectionString")] Contract contractChangeEvent, ILogger log)
        {
            _ = contractChangeEvent ?? throw new ArgumentNullException(nameof(contractChangeEvent));

            log?.LogInformation($"{nameof(ContractWithdrawnNotifierServiceBusFunction)} - ServiceBus topic triggered function for message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");

            await _contractStatusChangePublisher.NotifyContractWithdrawnAsync(contractChangeEvent);

            log?.LogInformation($"{nameof(ContractWithdrawnNotifierServiceBusFunction)} - ServiceBus topic trigger function processed message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");
        }
    }
}