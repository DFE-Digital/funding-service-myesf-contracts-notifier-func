using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.FeedReed
{
    /// <summary>
    /// Feed Read Exception Service.
    /// </summary>
    public interface IFeedReadExceptionService
    {
        /// <summary>
        /// Process and consume the feed read exception message.
        /// </summary>
        /// <param name="message">Feed read exception message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(FeedReadExceptionMessage message);
    }
}
