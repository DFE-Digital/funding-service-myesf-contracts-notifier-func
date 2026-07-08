using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FundingClaims
{
    /// <summary>
    /// Funding Claim Withdrawn Email function which listens and process the fundingclaimwithdrawnemail queue messages.
    /// </summary>
    public class FundingClaimWithdrawnEmailFunction
    {
        private readonly IFundingClaimWithdrawnEmailService _fundingClaimWithdrawnEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimWithdrawnEmailFunction"/> class.
        /// </summary>
        /// <param name="fundingClaimWithdrawnEmailService">Funding claim withdrawn email service.</param>
        public FundingClaimWithdrawnEmailFunction(IFundingClaimWithdrawnEmailService fundingClaimWithdrawnEmailService)
        {
            _fundingClaimWithdrawnEmailService = fundingClaimWithdrawnEmailService;
        }


        /// <summary>
        /// Listens to fundingclaimwithdrawnemail queue and send the email.
        /// </summary>
        /// <param name="fundingClaimWithdrawnEmailMessage">Funding claim withdrawn email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FundingClaimWithdrawnEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FundingClaimWithdrawnEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FundingClaimWithdrawnEmailMessage fundingClaimWithdrawnEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(fundingClaimWithdrawnEmailMessage.FundingClaimId)
                    .AsGuard<ArgumentException>();

                await _fundingClaimWithdrawnEmailService.Process(fundingClaimWithdrawnEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
