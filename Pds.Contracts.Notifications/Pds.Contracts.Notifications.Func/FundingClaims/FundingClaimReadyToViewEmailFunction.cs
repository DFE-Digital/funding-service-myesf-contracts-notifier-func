using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FundingClaims
{
    /// <summary>
    /// Funding Claim Ready To View Email function which listens and process the fundingclaimreadytoviewemail queue messages.
    /// </summary>
    public class FundingClaimReadyToViewEmailFunction
    {
        private readonly IFundingClaimReadyToViewEmailService _fundingClaimReadyToViewEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimReadyToViewEmailFunction"/> class.
        /// </summary>
        /// <param name="fundingClaimReadyToViewEmailService">Funding claim ready to view email service.</param>
        public FundingClaimReadyToViewEmailFunction(IFundingClaimReadyToViewEmailService fundingClaimReadyToViewEmailService)
        {
            _fundingClaimReadyToViewEmailService = fundingClaimReadyToViewEmailService;
        }

        /// <summary>
        /// Listens to fundingclaimreadytoviewemail queue and send the email.
        /// </summary>
        /// <param name="fundingClaimReadyToViewEmailMessage">Funding claim ready to view email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FundingClaimReadyToViewEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FundingClaimReadyToViewEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FundingClaimReadyToViewEmailMessage fundingClaimReadyToViewEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(fundingClaimReadyToViewEmailMessage.FundingClaimId)
                    .AsGuard<ArgumentException>();

                await _fundingClaimReadyToViewEmailService.Process(fundingClaimReadyToViewEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
