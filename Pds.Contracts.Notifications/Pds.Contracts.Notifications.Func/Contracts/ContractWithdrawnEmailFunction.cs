using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract withdrawn email function which listens and process the contractwithdrawnemail queue messages.
    /// </summary>
    public class ContractWithdrawnEmailFunction
    {
        private readonly IContractWithdrawnEmailService _contractWithdrawnEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractWithdrawnEmailFunction"/> class.
        /// </summary>
        /// <param name="contractWithdrawnEmailService">Contract withdrawn email service.</param>
        public ContractWithdrawnEmailFunction(IContractWithdrawnEmailService contractWithdrawnEmailService)
        {
            _contractWithdrawnEmailService = contractWithdrawnEmailService;
        }

        /// <summary>
        /// Listens to contractwithdrawnemail queue and send the email.
        /// </summary>
        /// <param name="contractWithdrawnEmailMessage">Contract withdrawn email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractWithdrawnEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractWithdrawnEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ContractWithdrawnEmailMessage contractWithdrawnEmailMessage)
        {
            try
            {
                ValidateServiceBusMessage(contractWithdrawnEmailMessage);

                await _contractWithdrawnEmailService.Process(contractWithdrawnEmailMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(ContractWithdrawnEmailMessage message)
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
