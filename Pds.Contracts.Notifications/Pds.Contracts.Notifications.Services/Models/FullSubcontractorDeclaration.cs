using System;
using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// FullSubcontractorDeclaration POCO.
    /// </summary>
    public class FullSubcontractorDeclaration
    {
        /// <summary>
        /// Gets or sets the data store id of this instance.
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Gets or sets who the subcontractor declaration is for.
        /// </summary>
        [Required]
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the version number of the Subcontractor declaration.
        /// </summary>
        [Required]
        public int Version { get; protected set; }

        /// <summary>
        /// Gets or sets the charity registration number of provider.
        /// </summary>
        public virtual string CharityRegistrationNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the organisation name of ukprn.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does provider subcontract any of their funded provision.
        /// </summary>
        public virtual bool SubContractFundedProvision { get; protected set; }

        /// <summary>
        /// Gets or sets when this instance was created.
        /// </summary>
        [Required]
        public virtual DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Gets or sets when this instance was last updated.
        /// </summary>
        [Required]
        public virtual DateTime LastUpdatedAt { get; protected set; }

        /// <summary>
        /// Gets or sets the web link.
        /// </summary>
        public virtual string WebLink { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether [second level sub contract funded provision].
        /// </summary>
        /// <value>
        /// <c>true</c> if [second level sub contract funded provision]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SecondLevelSubContractFundedProvision { get; protected set; }

        /// <summary>
        /// Gets or sets status of subcontractor declaration.
        /// </summary>
        public virtual SubcontractorDeclarationStatus Status { get; protected set; }

        /// <summary>
        /// Gets or sets the period of the subcontractor declaration.
        /// </summary>
        public string Period { get; protected set; }

        /// <summary>
        /// Gets or sets the datetime that the subcontractor declaration has been submitted by.
        /// </summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who submitted the subcontractor declaration.
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of subcontractor declaration.
        /// </summary>
        public virtual SubcontractorDeclarationSubmissionType SubcontractorDeclarationType { get; set; }

        /// <summary>
        /// Gets or sets if a subcontractor declaration has been submitted, then this will be populated by the display name of the provider agent who approved it.
        /// </summary>
        public string SubmittedByDisplayName { get; set; }
    }
}
