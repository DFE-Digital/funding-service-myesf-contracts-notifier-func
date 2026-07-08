using Microsoft.Extensions.Configuration;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Pds.Contracts.Notifications.Services.Implementations.FeedReed
{
    /// <summary>
    /// Feed Read Exception Service.
    /// </summary>
    public class FeedReadExceptionService : IFeedReadExceptionService
    {
        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FeedReadExceptionService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReadExceptionService"/> class.
        /// </summary>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="logger">Logger.</param>
        public FeedReadExceptionService(
            INotificationEmailQueueService notificationEmailQueueService,
            IConfiguration configuration,
            ILoggerAdapter<FeedReadExceptionService> logger)
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(FeedReadExceptionMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FeedReadExceptionMessage)} for message type [{nameof(message.Type)}], bookmark Id [{message.Bookmark}].");

            var emailTemplateName = GetEmailTemplateName(message);

            string cdsUserExceptionEmail = _configuration.GetValue<string>("CdsUserExceptionEmail");

            if (string.IsNullOrEmpty(cdsUserExceptionEmail))
            {
                var errorMessage = $"{nameof(FeedReadExceptionMessage)}-[{message.Type}]-[{message.Bookmark}], Unable to find the email address from configuration->cdsUserExceptionEmail.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var personalisation = new Dictionary<string, object>()
            {
                 { "Bookmark",  message.Bookmark },
                 { "Url",  message.Url }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(
                new List<string> { cdsUserExceptionEmail },
                Constants.RequestingService_FundingClaims,
                emailTemplateName,
                personalisation);

            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation($"Processing {nameof(FeedReadExceptionMessage)} for message type [{nameof(message.Type)}], bookmark Id [{message.Bookmark}] and published to SharedEmailprocessorQueue.");
        }

        /// <summary>
        /// Message type for constructiong NotificationMessage object.
        /// </summary>
        /// <param name="message">Service bus message.</param>
        /// <returns>Message type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid message type.</exception>
        private static string GetEmailTemplateName(FeedReadExceptionMessage message)
        {
            return message.Type switch
            {
                ExceptionType.BookmarkNotMatched => Constants.MessageType_FeedReadExceptionBookmarkNotMatchedEmail,
                ExceptionType.EmptyPageOnFeed => Constants.MessageType_FeedReadExceptionEmptyPageEmail,
                _ => throw new ArgumentOutOfRangeException($"No email template found for expection type [{message.Type.ToString()}]")
            };
        }
    }
}