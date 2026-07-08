namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Message for reconcilaition that is ready to be viewed.
    /// </summary>
    public class ReconciliationReadyToBeViewedEmailMessage
    {
        /// <summary>
        /// Gets or sets the id of the reconcilaition funding claim that is ready to be viewed.
        /// </summary>
        public int ReconciliationId { get; set; }
    }
}
