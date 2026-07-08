namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message detailing a change in a funding claim status.
    /// </summary>
    public class FundingClaimSignedEmailMessage
    {
        /// <summary>
        /// Gets or sets the id of the funding claim that has changed state.
        /// </summary>
        public int FundingClaimId { get; set; }
    }
}
