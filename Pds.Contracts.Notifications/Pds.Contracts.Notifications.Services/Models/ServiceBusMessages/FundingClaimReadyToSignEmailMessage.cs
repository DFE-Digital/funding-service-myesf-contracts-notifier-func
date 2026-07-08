namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Message for funding claim that is in ready to sign state.
    /// </summary>
    public class FundingClaimReadyToSignEmailMessage
    {
        /// <summary>
        /// Gets or sets the id of the funding claim that is ready to sign state.
        /// </summary>
        public int FundingClaimId { get; set; }
    }
}
