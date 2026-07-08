using System;

namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message detailing an exception that occurred during the process of a the feed.
    /// </summary>
    public class FeedReadThresholdExceededWarningMessage
    {
        /// <summary>
        /// Gets or sets the time the Feed started to be read.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the time the warning was raised.
        /// </summary>
        public DateTime Now { get; set; }

        /// <summary>
        /// Gets or sets the bookmark that is being looked for.
        /// </summary>
        public Guid BookmarkId { get; set; }

        /// <summary>
        /// Gets or sets the URL that was last read.
        /// </summary>
        public string LastPageUrl { get; set; }
    }
}
