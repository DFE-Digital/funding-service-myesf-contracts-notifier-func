using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract Approved Email Function which listens and process the contractcontenttobesigned queue messages.
    /// </summary>
    public class ContractContentToBeSignedFunction
    {
        private readonly IContractContentToBeSignedService _contractContentToBeSignedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractContentToBeSignedFunction"/> class.
        /// </summary>
        /// <param name="contractContentToBeSignedService">Contract content to be signed service.</param>
        public ContractContentToBeSignedFunction(IContractContentToBeSignedService contractContentToBeSignedService)
        {
            _contractContentToBeSignedService = contractContentToBeSignedService;
        }

        /// <summary>
        /// Listens to contractcontenttobesigned queue and send the email.
        /// </summary>
        /// <param name="contractContentToBeSignedMessage">Contract content to be signed message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractContentToBeSignedFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractContentToBeSignedQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ContractContentToBeSignedMessage contractContentToBeSignedMessage)
        {
            try
            {
                It.IsNullOrDefault(contractContentToBeSignedMessage.ContractId)
                    .AsGuard<ArgumentException>();

                await _contractContentToBeSignedService.Process(contractContentToBeSignedMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
