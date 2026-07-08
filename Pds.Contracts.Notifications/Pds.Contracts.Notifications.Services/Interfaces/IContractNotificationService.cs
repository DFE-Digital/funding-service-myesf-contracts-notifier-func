using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Contract reminder service.
    /// </summary>
    public interface IContractNotificationService
    {
        /// <summary>
        /// Retrieves a list of contracts that are overdue for signing.
        /// </summary>
        /// <returns>Async task.</returns>
        Task<ContractReminders> GetOverdueContracts();

        /// <summary>
        /// Queues a reminder email to be sent for the contract.
        /// </summary>
        /// <param name="contract">Contract to send a reminder for.</param>
        /// <returns>Async task.</returns>
        Task QueueContractEmailReminderMessage(Contract contract);

        /// <summary>
        /// Notifies about changes to the contract.
        /// </summary>
        /// <param name="contractChange">Changed contract.</param>
        /// <returns>Async task.</returns>
        Task NotifyContractReminderSent(Contract contractChange);
    }
}