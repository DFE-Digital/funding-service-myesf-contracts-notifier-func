using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.FundingClaims
{
    /// <summary>
    /// Reconciliation Ready To Be Viewed Email Service.
    /// </summary>
    public interface IReconciliationReadyToBeViewedEmailService
    {
        /// <summary>
        /// Process and consume the reconciliation ready to be viewed email message.
        /// </summary>
        /// <param name="message">Reconciliation ready to be viewed email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ReconciliationReadyToBeViewedEmailMessage message);
    }
}
