using System;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Reconciliation claim object.
    /// </summary>
    public class Reconciliation
    {
        /// <summary>
        /// Gets or sets reconciliation Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets who the reconciliation is for.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of a reconciliation.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the version number of the reconciliation.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the type of reconciliation that this instance represents.
        /// </summary>
        public ReconciliationType Type { get; set; }

        /// <summary>
        /// Gets or sets the period for which this reconciliation belongs.
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Gets or sets when this instance was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when this instance was last updated.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether or not this reconciliation has passed validation.
        /// </summary>
        public bool? IsValid { get; set; }
    }
}
