using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FundingClaims
{
    /// <summary>
    /// Funding Claim Ready To Sign Email function which listens and process the fundingclaimreadytosignemail queue messages.
    /// </summary>
    public class FundingClaimReadyToSignEmailFunction
    {
        private readonly IFundingClaimReadyToSignEmailService _fundingClaimReadyToSignEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimReadyToSignEmailFunction"/> class.
        /// </summary>
        /// <param name="fundingClaimReadyToSignEmailService">Funding claim ready to sign email service.</param>
        public FundingClaimReadyToSignEmailFunction(IFundingClaimReadyToSignEmailService fundingClaimReadyToSignEmailService)
        {
            _fundingClaimReadyToSignEmailService = fundingClaimReadyToSignEmailService;
        }

        /// <summary>
        /// Listens to fundingclaimreadytosignemail queue and send the email.
        /// </summary>
        /// <param name="fundingClaimReadyToSignEmailMessage">Funding claim ready to sign email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FundingClaimReadyToSignEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FundingClaimReadyToSignEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FundingClaimReadyToSignEmailMessage fundingClaimReadyToSignEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(fundingClaimReadyToSignEmailMessage.FundingClaimId)
                    .AsGuard<ArgumentException>();

                await _fundingClaimReadyToSignEmailService.Process(fundingClaimReadyToSignEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
