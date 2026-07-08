using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Process Contract From Feed Exception Service.
    /// </summary>
    public interface IProcessContractFromFeedExceptionService
    {
        /// <summary>
        /// Process and consume the contract from feed exception message.
        /// </summary>
        /// <param name="message">Process contract from feed exception message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ProcessContractFromFeedExceptionMessage message);
    }
}
