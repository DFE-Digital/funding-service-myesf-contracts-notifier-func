using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Withdrawn Email Service.
    /// </summary>
    public interface IContractWithdrawnEmailService
    {
        /// <summary>
        /// Process and consume the contract withdrawn email message.
        /// </summary>
        /// <param name="message">Contract withdrawn email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractWithdrawnEmailMessage message);
    }
}
