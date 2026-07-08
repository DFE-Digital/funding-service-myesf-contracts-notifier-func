using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// A paged collection of contracts that need a reminder.
    /// </summary>
    public class ContractReminders
    {
        /// <summary>
        /// Gets or sets list of contracts.
        /// </summary>
        public IList<Contract> Contracts { get; set; }
    }
}
