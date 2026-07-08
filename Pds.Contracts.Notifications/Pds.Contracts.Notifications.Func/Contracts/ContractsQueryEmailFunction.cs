using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contracts query email function which listens and process the contractsqueryemail queue messages.
    /// </summary>
    public class ContractsQueryEmailFunction
    {
        private readonly IContractsQueryEmailService _contractsQueryEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsQueryEmailFunction"/> class.
        /// </summary>
        /// <param name="contractsQueryEmailService">Contracts Query Email Service.</param>
        public ContractsQueryEmailFunction(IContractsQueryEmailService contractsQueryEmailService)
        {
            _contractsQueryEmailService = contractsQueryEmailService;
        }

        /// <summary>
        /// Listens to contractsqueryemail queue and send the email.
        /// </summary>
        /// <param name="contractsQueryEmailMessage">Contracts query email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractsQueryEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractsQueryEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ContractsQueryEmailMessage contractsQueryEmailMessage)
        {
            try
            {
                ValidateServiceBusMessage(contractsQueryEmailMessage);

                await this._contractsQueryEmailService.Process(contractsQueryEmailMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(ContractsQueryEmailMessage message)
        {
            It.IsEmpty(message.ProviderUserName)
            .AsGuard<ArgumentNullException>();

            It.IsEmpty(message.ProviderName)
            .AsGuard<ArgumentNullException>();

            It.IsEmpty(message.ProviderEmailAddress)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.ContractId)
            .AsGuard<ArgumentException>();

            It.IsEmpty(message.QueryReason)
            .AsGuard<ArgumentNullException>();

            It.IsEmpty(message.QueryDetail)
            .AsGuard<ArgumentNullException>();
        }
    }
}
