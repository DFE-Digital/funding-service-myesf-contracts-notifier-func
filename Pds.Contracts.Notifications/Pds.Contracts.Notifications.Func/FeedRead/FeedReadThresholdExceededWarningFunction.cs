using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FeedReed
{
    /// <summary>
    /// Feed read threshold exceeded warning Function which listens and process the feedreadthresholdexceededwarning queue messages.
    /// </summary>
    public class FeedReadThresholdExceededWarningFunction
    {
        private readonly IFeedReadThresholdExceededWarningService _feedReadThresholdExceededWarningService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReadThresholdExceededWarningFunction"/> class.
        /// </summary>
        /// <param name="feedReadThresholdExceededWarningService">Feed Read Threshold Exceeded Warning Service.</param>
        public FeedReadThresholdExceededWarningFunction(IFeedReadThresholdExceededWarningService feedReadThresholdExceededWarningService)
        {
            _feedReadThresholdExceededWarningService = feedReadThresholdExceededWarningService;
        }

        /// <summary>
        /// Listens to feedReadthresholdexceededwarning queue and send the email.
        /// </summary>
        /// <param name="feedReadThresholdExceededWarningMessage">Feed read threshold exceeded warning message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FeedReadThresholdExceededWarningFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FeedReadThresholdExceededWarningEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FeedReadThresholdExceededWarningMessage feedReadThresholdExceededWarningMessage)
        {
            try
            {
                ValidateServiceBusMessage(feedReadThresholdExceededWarningMessage);

                await this._feedReadThresholdExceededWarningService.Process(feedReadThresholdExceededWarningMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(FeedReadThresholdExceededWarningMessage message)
        {
            It.IsNullOrDefault(message.Start)
            .AsGuard<ArgumentException>();

            It.IsNullOrDefault(message.Now)
            .AsGuard<ArgumentException>();

            It.IsNullOrDefault(message.BookmarkId)
                .AsGuard<ArgumentException>();

            It.IsEmpty(message.LastPageUrl)
                .AsGuard<ArgumentNullException>();
        }
    }
}
