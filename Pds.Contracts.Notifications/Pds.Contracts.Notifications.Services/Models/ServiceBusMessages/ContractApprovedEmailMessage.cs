namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Message for contract signed.
    /// </summary>
    public class ContractApprovedEmailMessage
    {
        /// <summary>
        /// Gets or sets the Id of the contract that has changed state.
        /// </summary>
        public int ContractId { get; set; }
    }
}
