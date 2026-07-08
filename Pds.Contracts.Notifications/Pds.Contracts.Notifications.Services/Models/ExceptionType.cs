using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// The type of exception that was thrown whilst reading the feed.
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Bookmark not matched
        /// </summary>
        [Display(Name = "Bookmark not matched")]
        BookmarkNotMatched = 0,

        /// <summary>
        /// Empty page on feed
        /// </summary>
        [Display(Name = "Empty page on feed")]
        EmptyPageOnFeed = 1
    }
}
