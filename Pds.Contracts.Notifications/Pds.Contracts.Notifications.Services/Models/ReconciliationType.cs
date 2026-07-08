using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Defines the type that a Reconciliation can be.
    /// </summary>
    public enum ReconciliationType
    {
        /// <summary>
        /// The Reconciliation is a mid-year reconciliation.
        /// </summary>
        [Display(Name = "Mid year")]
        MIDYEAR = 0,

        /// <summary>
        /// The Reconciliation is a year-end reconciliation.
        /// </summary>
        [Display(Name = "Year end")]
        YEAREND = 1,

        /// <summary>
        /// The Reconciliation is a final reconciliation.
        /// </summary>
        [Display(Name = "Final")]
        FINAL = 2
    }
}
