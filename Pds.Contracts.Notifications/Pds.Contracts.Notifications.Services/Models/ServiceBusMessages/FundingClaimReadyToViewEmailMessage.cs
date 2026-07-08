namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Message for funding claim that is in ready to view state.
    /// </summary>
    public class FundingClaimReadyToViewEmailMessage
    {
        /// <summary>
        /// Gets or sets the id of the funding claim that is ready to view state.
        /// </summary>
        public int FundingClaimId { get; set; }
    }
}
