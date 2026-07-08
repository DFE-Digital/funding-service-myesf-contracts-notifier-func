namespace Sfa.Sfs.Contracts.Messaging
{
    /// <summary>
    /// Contract approved message.
    /// </summary>
    public class ContractApprovedMessage
    {
        /// <summary>
        /// Gets or sets contract id required by legacy message processor.
        /// </summary>
        public int ContractId { get; set; }
    }
}