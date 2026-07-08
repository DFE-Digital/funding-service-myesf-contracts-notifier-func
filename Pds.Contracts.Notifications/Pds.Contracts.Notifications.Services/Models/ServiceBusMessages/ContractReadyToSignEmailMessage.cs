namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Contract Ready To Sign Email Message.
    /// </summary>
    public class ContractReadyToSignEmailMessage
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
