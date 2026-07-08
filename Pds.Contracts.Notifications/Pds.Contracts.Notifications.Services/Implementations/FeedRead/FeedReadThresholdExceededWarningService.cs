using Microsoft.Extensions.Configuration;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.FeedReed
{
    /// <summary>
    /// Feed Read Threshold Exceeded Warning Service.
    /// </summary>
    public class FeedReadThresholdExceededWarningService : IFeedReadThresholdExceededWarningService
    {
        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FeedReadThresholdExceededWarningService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReadThresholdExceededWarningService"/> class.
        /// </summary>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="logger">Logger.</param>
        public FeedReadThresholdExceededWarningService(
            INotificationEmailQueueService notificationEmailQueueService,
            IConfiguration configuration,
            ILoggerAdapter<FeedReadThresholdExceededWarningService> logger)
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(FeedReadThresholdExceededWarningMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FeedReadThresholdExceededWarningMessage)} for bookmark Id {message.BookmarkId}.");

            string cdsUserExceptionEmail = _configuration.GetValue<string>("CdsUserExceptionEmail");

            if (string.IsNullOrEmpty(cdsUserExceptionEmail))
            {
                var errorMessage = $"{nameof(FeedReadThresholdExceededWarningMessage)}-Bookmarkid [{message.BookmarkId}],Unable to find the email address from configuration->cdsUserExceptionEmail.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var personalisation = new Dictionary<string, object>()
            {
                 { "Start",  message.Start.DisplayFormat() },
                 { "TimeWarningRaised",  message.Now.DisplayFormat() },
                 { "Bookmark",  message.BookmarkId },
                 { "Url",  message.LastPageUrl }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(
                new List<string> { cdsUserExceptionEmail },
                Constants.RequestingService_FundingClaims,
                Constants.MessageType_FeedReadThresholdExceededWarningEmail,
                personalisation);

            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation($"Processing {nameof(FeedReadThresholdExceededWarningMessage)} for bookmark Id {message.BookmarkId} and published to SharedEmailprocessorQueue.");
        }
    }
}
