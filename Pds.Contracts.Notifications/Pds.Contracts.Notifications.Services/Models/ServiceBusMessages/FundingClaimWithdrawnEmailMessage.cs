namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Message for funding claim that has been withdrawn.
    /// </summary>
    public class FundingClaimWithdrawnEmailMessage
    {
        /// <summary>
        /// Gets or sets the id of the funding claim that has been withdrawn.
        /// </summary>
        public int FundingClaimId { get; set; }
    }
}
