namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Contract Ready To Review Email Message.
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
