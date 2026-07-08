namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// A request object to send to the Data API to update contracts.
    /// </summary>
    public class ContractUpdateRequest
    {
        /// <summary>
        /// Gets or sets the contract id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        public int ContractVersion { get; set; }
    }
}
