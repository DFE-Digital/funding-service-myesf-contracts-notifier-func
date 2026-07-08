namespace Sfa.Sfs.Contracts.Messaging
{
    /// <summary>
    /// Contract approved message.
    /// </summary>
    public class ContractReadyToReviewEmailMessage
    {
        /// <summary>
        /// Gets or sets contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets contract version number.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets ukprn of provider.
        /// </summary>
        public int Ukprn { get; set; }
    }
}