namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message for a reminder contract.
    /// </summary>
    public class ContractReminderEmailMessage
    {
        /// <summary>
        /// Gets or sets the Id of the contract that still needs to be signed (email reminder).
        /// </summary>
        public int ContractId { get; set; }
    }
}
