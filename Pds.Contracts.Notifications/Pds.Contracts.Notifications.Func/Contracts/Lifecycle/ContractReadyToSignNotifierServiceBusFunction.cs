using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;

namespace Pds.Contracts.Notifications.Func.Contracts.Lifecycle
{
    /// <summary>
    /// Contracts ready to sign notification service bus function.
    /// </summary>
    public class ContractReadyToSignNotifierServiceBusFunction
    {
        private readonly IContractStatusChangePublisher _contractStatusChangePublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToSignNotifierServiceBusFunction"/> class.
        /// </summary>
        /// <param name="contractStatusChangePublisher">An implementation of <see cref="IContractStatusChangePublisher"/>.</param>
        public ContractReadyToSignNotifierServiceBusFunction(IContractStatusChangePublisher contractStatusChangePublisher)
        {
            _contractStatusChangePublisher = contractStatusChangePublisher;
        }

        /// <summary>
        /// Process contract change event for ready to sign messages.
        /// </summary>
        /// <param name="contractChangeEvent">Contract change event.</param>
        /// <param name="log">for log output.</param>
        /// <returns>A task completetion from asynchronous operation.</returns>
        [Function(nameof(ContractReadyToSignNotifierServiceBusFunction))]
        public async Task RunAsync([ServiceBusTrigger("%Pds.Contracts.Notifications.Topic%", "%Pds.Contracts.ReadyToSign.Subscription%", Connection = "AzureMessagingServiceBusOptions:ConnectionString")] Contract contractChangeEvent, ILogger log)
        {
            _ = contractChangeEvent ?? throw new ArgumentNullException(nameof(contractChangeEvent));

            log?.LogInformation($"{nameof(ContractReadyToSignNotifierServiceBusFunction)} - ServiceBus topic triggered function for message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");

            await _contractStatusChangePublisher.NotifyContractIsReadyToSignAsync(contractChangeEvent);

            log?.LogInformation($"{nameof(ContractReadyToSignNotifierServiceBusFunction)} - ServiceBus topic trigger function processed message with contract number: {contractChangeEvent.ContractNumber}, version: {contractChangeEvent.ContractVersion} with status {contractChangeEvent.Status}");
        }
    }
}