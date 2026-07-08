using System;

namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message detailing an exception that occurred during the process of a the feed.
    /// </summary>
    public class FeedReadExceptionMessage
    {
        /// <summary>
        /// Gets or sets the type of exception.
        /// </summary>
        public ExceptionType Type { get; set; }

        /// <summary>
        /// Gets or sets the bookmark that was not matched.
        /// </summary>
        public Guid Bookmark { get; set; }

        /// <summary>
        /// Gets or sets the url that was being used at the time.
        /// </summary>
        public string Url { get; set; }
    }
}
