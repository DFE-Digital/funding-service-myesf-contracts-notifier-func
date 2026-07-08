using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.FundingClaims
{
    /// <summary>
    /// Funding Claim Ready To View Email Service.
    /// </summary>
    public interface IFundingClaimReadyToViewEmailService
    {
        /// <summary>
        /// Process and consume the funding claim ready to View email message.
        /// </summary>
        /// <param name="message">Funding claim ready to View email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(FundingClaimReadyToViewEmailMessage message);
    }
}
