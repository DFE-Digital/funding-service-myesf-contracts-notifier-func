using Pds.Core.Notification.Models;
using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Utilities
{
    /// <summary>
    /// Helper class for Email Notification.
    /// </summary>
    public static class EmailNotificationHelper
    {
        /// <summary>
        /// Constructs the notification message for sending NotificationEmailQueueService.
        /// </summary>
        /// <param name="toEmails">To Emails.</param>
        /// <param name="requestingService">Requesting Service.</param>
        /// <param name="emailTemplateName">Email Template Name.</param>
        /// <param name="personalisation">Personalisation.</param>
        /// <returns>NotificationMessage.</returns>
        public static NotificationMessage ConstructNotification(IEnumerable<string> toEmails, string requestingService, string emailTemplateName, Dictionary<string, object> personalisation = null)
        {
            return new NotificationMessage()
            {
                EmailAddresses = toEmails,
                RequestingService = requestingService,
                EmailMessageType = emailTemplateName,
                EmailPersonalisation = new GovUkNotifyPersonalisation
                {
                    Personalisation = personalisation
                }
            };
        }
    }
}
