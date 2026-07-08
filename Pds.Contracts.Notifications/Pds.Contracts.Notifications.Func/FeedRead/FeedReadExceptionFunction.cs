using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FeedReed
{
    /// <summary>
    /// Feed Read Exception Function which listens and process the feedreadexception queue messages.
    /// </summary>
    public class FeedReadExceptionFunction
    {
        private readonly IFeedReadExceptionService _feedReadExceptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReadExceptionFunction"/> class.
        /// </summary>
        /// <param name="feedReadExceptionService">Feed Read Exception Service.</param>
        public FeedReadExceptionFunction(IFeedReadExceptionService feedReadExceptionService)
        {
            _feedReadExceptionService = feedReadExceptionService;
        }

        /// <summary>
        /// Listens to feedreadexception queue and send the email.
        /// </summary>
        /// <param name="feedReadExceptionMessage">Feed read exception message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(FeedReadExceptionFunction))]
        public async Task Run([ServiceBusTrigger(Constants.FeedReadExceptionEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] FeedReadExceptionMessage feedReadExceptionMessage)
        {
            try
            {
                ValidateServiceBusMessage(feedReadExceptionMessage);

                await this._feedReadExceptionService.Process(feedReadExceptionMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(FeedReadExceptionMessage message)
        {
            (!Enum.IsDefined(typeof(ExceptionType), message.Type))
            .AsGuard<ArgumentException>();

            It.IsNullOrDefault(message.Bookmark)
            .AsGuard<ArgumentException>();

            It.IsEmpty(message.Url)
                .AsGuard<ArgumentNullException>();
        }
    }
}
