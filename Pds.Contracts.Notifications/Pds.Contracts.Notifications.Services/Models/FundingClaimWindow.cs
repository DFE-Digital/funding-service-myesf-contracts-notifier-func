using System;
using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Represents a window in which Funding claims within DC can be submitted and then signed within SFS.
    /// </summary>
    public partial class FundingClaimWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimWindow"/> class.
        /// Default constructor.
        /// </summary>
        public FundingClaimWindow()
        {
            FundingClaims = new HashSet<FundingClaim>();
        }

        /// <summary>
        /// Gets or sets identifier for the funding claim window.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Data Set version id from DEDS.
        /// </summary>
        public Guid DataSetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies which Data Collection that this data was related to within DEDS.
        /// </summary>
        public string DataCollectionKey { get; set; }

        /// <summary>
        /// Gets or sets the time at which Providers can begin to enter and submit their final data into DC.
        /// </summary>
        public DateTime SubmissionOpenDate { get; set; }

        /// <summary>
        /// Gets or sets the time at which Providers can no longer enter and submit their final data into DC.
        /// </summary>
        public DateTime SubmissionCloseDate { get; set; }

        /// <summary>
        /// Gets or sets the time at which Providers can no longer sign their funding claims within SFS.
        /// </summary>
        public DateTime? SignatureCloseDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the funding claims associated with this window should be signed.
        /// </summary>
        public bool RequiresSignature { get; set; }

        /// <summary>
        /// Gets or sets when the instance was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the instance was last modified.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the latest funding claims have been fetched for the window.
        /// </summary>
        public bool FundingClaimsRetrieved { get; set; }

        /// <summary>
        /// Gets or sets all funding claims in the window.
        /// </summary>
        public virtual ICollection<FundingClaim> FundingClaims { get; set; }
    }
}
