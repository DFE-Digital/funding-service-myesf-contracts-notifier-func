using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract Ready To Review Email Function which listens and process the contractreadytoreviewemail queue messages.
    /// </summary>
    public class ContractReadyToReviewEmailFunction
    {
        private readonly IContractReadyToReviewEmailService _contractReadyToReviewEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToReviewEmailFunction"/> class.
        /// </summary>
        /// <param name="contractReadyToReviewEmailService">Contract ready to review email service.</param>
        public ContractReadyToReviewEmailFunction(IContractReadyToReviewEmailService contractReadyToReviewEmailService)
        {
            _contractReadyToReviewEmailService = contractReadyToReviewEmailService;
        }

        /// <summary>
        /// Listens to contractreadytoreviewemail queue and send the email.
        /// </summary>
        /// <param name="contractReadyToReviewEmailMessage">Contract ready to review email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ContractReadyToReviewEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractReadyToReviewEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ContractReadyToReviewEmailMessage contractReadyToReviewEmailMessage)
        {
            try
            {
                ValidateServiceBusMessage(contractReadyToReviewEmailMessage);

                await _contractReadyToReviewEmailService.Process(contractReadyToReviewEmailMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(ContractReadyToReviewEmailMessage message)
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
