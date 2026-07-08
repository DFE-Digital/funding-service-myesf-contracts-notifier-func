using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract Ready To Sign Email Function which listens and process the contractreadytosignemail queue messages.
    /// </summary>
    public class ContractReadyToSignEmailFunction
    {
        private readonly IContractReadyToSignEmailService _contractReadyToSignEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToSignEmailFunction"/> class.
        /// </summary>
        /// <param name="contractReadyToSignEmailService">Contract ready to sign email service.</param>
        public ContractReadyToSignEmailFunction(IContractReadyToSignEmailService contractReadyToSignEmailService)
        {
            _contractReadyToSignEmailService = contractReadyToSignEmailService;
        }

        /// <summary>
        /// Listens to contractreadytosignemail queue and send the email.
        /// </summary>
        /// <param name="contractReadyToSignEmailMessage">Contract ready to sign email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractReadyToSignEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractReadyToSignEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")]ContractReadyToSignEmailMessage contractReadyToSignEmailMessage)
        {
            try
            {
                ValidateServiceBusMessage(contractReadyToSignEmailMessage);

                await _contractReadyToSignEmailService.Process(contractReadyToSignEmailMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(ContractReadyToSignEmailMessage message)
        {
            It.IsEmpty(message.ContractNumber)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.VersionNumber)
            .AsGuard<ArgumentException>();

            It.IsNullOrDefault(message.Ukprn)
                .AsGuard<ArgumentException>();
        }
    }
}
