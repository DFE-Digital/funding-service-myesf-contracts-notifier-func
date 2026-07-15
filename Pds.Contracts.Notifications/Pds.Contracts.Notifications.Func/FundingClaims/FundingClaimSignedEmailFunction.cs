using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FundingClaims
{
    /// <summary>
    /// Funding Claim Signed Email function which listens and process the fundingclaimsignedemail queue messages.
    /// </summary>
    public class FundingClaimSignedEmailFunction
    {
        private readonly IFundingClaimSignedEmailService _fundingClaimSignedEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimSignedEmailFunction"/> class.
        /// </summary>
        /// <param name="fundingClaimSignedEmailService">Funding claim signed email service.</param>
        public FundingClaimSignedEmailFunction(IFundingClaimSignedEmailService fundingClaimSignedEmailService)
        {
            _fundingClaimSignedEmailService = fundingClaimSignedEmailService;
        }

        /// <summary>
        /// Listens to fundingclaimsignedemail queue and send the email.
        /// </summary>
        /// <param name="fundingClaimSignedEmailMessage">Funding claim signed email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FundingClaimSignedEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FundingClaimSignedEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FundingClaimSignedEmailMessage fundingClaimSignedEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(fundingClaimSignedEmailMessage.FundingClaimId)
                    .AsGuard<ArgumentException>();

                await _fundingClaimSignedEmailService.Process(fundingClaimSignedEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
