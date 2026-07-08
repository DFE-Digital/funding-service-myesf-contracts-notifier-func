namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Represents the type of declaration that the provider is submitting.
    /// </summary>
    public enum SubcontractorDeclarationSubmissionType
    {
        /// <summary>
        /// The Subcontractor Declaration is a nil declaration.
        /// </summary>
        Nil = 0,

        /// <summary>
        /// The Subcontractor Declaration is a full declaration.
        /// </summary>
        Full = 1
    }
}