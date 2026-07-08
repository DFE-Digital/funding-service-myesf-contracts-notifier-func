namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Contract Content To Be SignedMessage.
    /// </summary>
    public class ContractContentToBeSignedMessage
    {
        /// <summary>
        /// Gets or sets the id of the contract that has changed state.
        /// </summary>
        public int ContractId { get; set; }
    }
}
