using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Reminder Email Service.
    /// </summary>
    public interface IContractReminderEmailService
    {
        /// <summary>
        /// Process and consume the contract reminder email message.
        /// </summary>
        /// <param name="message">Contract reminder email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractReminderEmailMessage message);
    }
}
