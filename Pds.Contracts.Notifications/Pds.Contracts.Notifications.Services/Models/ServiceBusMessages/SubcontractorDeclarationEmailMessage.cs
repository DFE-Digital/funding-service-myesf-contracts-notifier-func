namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Subcontractor declaration submission message.
    /// </summary>
    public class SubcontractorDeclarationEmailMessage
    {
        /// <summary>
        /// Gets or sets the Id of the subcontractor declaration that has been submitted.
        /// </summary>
        public int SubcontractorDeclarationId { get; set; }
    }
}
