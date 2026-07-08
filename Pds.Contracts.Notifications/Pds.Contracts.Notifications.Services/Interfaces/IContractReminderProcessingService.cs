using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Interface for a service to allow processing of contract reminders.
    /// </summary>
    public interface IContractReminderProcessingService
    {
        /// <summary>
        /// Queries for and issues reminders for contracts that are overdue for signing.
        /// </summary>
        /// <returns>Async task.</returns>
        Task IssueContractReminders();
    }
}