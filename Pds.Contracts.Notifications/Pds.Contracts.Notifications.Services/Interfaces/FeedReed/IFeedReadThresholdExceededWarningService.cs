using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.FeedReed
{
    /// <summary>
    /// Feed Read Threshold Exceeded Warning Service.
    /// </summary>
    public interface IFeedReadThresholdExceededWarningService
    {
        /// <summary>
        /// Process and consume the feed read threshold exceeded warning message.
        /// </summary>
        /// <param name="message">Feed read threshold exceeded warning message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(FeedReadThresholdExceededWarningMessage message);
    }
}
