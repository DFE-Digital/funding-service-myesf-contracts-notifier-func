using System;

namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message detailing an exception that occurred during the process of a the feed.
    /// </summary>
    public class ProcessContractFromFeedExceptionMessage
    {
        /// <summary>
        /// Gets or sets the parent status of the contract element in the feed that caused the exception.
        /// </summary>
        public string ParentFeedStatus { get; set; }

        /// <summary>
        /// Gets or sets the status of the contract element in the feed that caused the exception.
        /// </summary>
        public string FeedStatus { get; set; }

        /// <summary>
        /// Gets or sets the status of the contract in the system at the time of the exception.
        /// </summary>
        public string ExistingContractStatus { get; set; }

        /// <summary>
        /// Gets or sets the parent contract number of the contract that caused the exception.
        /// </summary>
        public string ParentContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract number of the contract that caused the exception.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract number of the contract that caused the exception.
        /// </summary>
        public int ContractVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the title (friendly name) of the contract that caused the exception.
        /// </summary>
        public string ContractTitle { get; set; }

        /// <summary>
        /// Gets or sets the time that the exception happened.
        /// </summary>
        public DateTime ExceptionTime { get; set; }

        /// <summary>
        /// Gets or sets the provider name for the contract that caused the exception.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the UKPRN of the contract that caused the exception.
        /// </summary>
        public int Ukprn { get; set; }
    }
}
