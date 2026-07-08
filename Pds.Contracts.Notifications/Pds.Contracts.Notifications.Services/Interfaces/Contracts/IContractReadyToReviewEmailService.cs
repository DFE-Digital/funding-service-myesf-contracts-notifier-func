using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Ready To Review Email Service.
    /// </summary>
    public interface IContractReadyToReviewEmailService
    {
        /// <summary>
        /// Process and consume the contract ready to review email message.
        /// </summary>
        /// <param name="message">Contract ready to review email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractReadyToReviewEmailMessage message);
    }
}
