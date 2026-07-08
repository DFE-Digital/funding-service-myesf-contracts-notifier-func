using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract Reminder Email Function which listens and process the contractreminderemail queue messages.
    /// </summary>
    public class ContractReminderEmailFunction
    {
        private readonly IContractReminderEmailService _contractReminderEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReminderEmailFunction"/> class.
        /// </summary>
        /// <param name="contractReminderEmailService">Contract reminder email service.</param>
        public ContractReminderEmailFunction(IContractReminderEmailService contractReminderEmailService)
        {
            _contractReminderEmailService = contractReminderEmailService;
        }

        /// <summary>
        /// Listens to contractreminderemail queue and send the email.
        /// </summary>
        /// <param name="contractReminderEmailMessage">Contract reminder email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractReminderEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractReminderEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ContractReminderEmailMessage contractReminderEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(contractReminderEmailMessage.ContractId)
                    .AsGuard<ArgumentException>();

                await _contractReminderEmailService.Process(contractReminderEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
