namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Represents the states a full subcontractor declaration can be in.
    /// </summary>
    public enum SubcontractorDeclarationStatus
    {
        /// <summary>
        /// The Subcontractor Declarations is in draft mode.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// The Subcontractor Declarations has been approved by provider.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// The Subcontractor Declarations has been closed by SFS service.
        /// </summary>
        Closed = 2
    }
}