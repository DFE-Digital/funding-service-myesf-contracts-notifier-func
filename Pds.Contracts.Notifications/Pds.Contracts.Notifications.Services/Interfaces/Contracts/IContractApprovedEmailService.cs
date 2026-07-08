using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Approved Email Service.
    /// </summary>
    public interface IContractApprovedEmailService
    {
        /// <summary>
        /// Process and consume the contract approved email message.
        /// </summary>
        /// <param name="message">Contract approved email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractApprovedEmailMessage message);
    }
}
