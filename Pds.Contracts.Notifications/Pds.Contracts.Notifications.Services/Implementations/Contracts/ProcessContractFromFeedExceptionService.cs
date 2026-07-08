using Microsoft.Extensions.Configuration;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Process Contract From Feed Exception Service.
    /// </summary>
    public class ProcessContractFromFeedExceptionService : IProcessContractFromFeedExceptionService
    {
        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ProcessContractFromFeedExceptionService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContractFromFeedExceptionService"/> class.
        /// </summary>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="logger">Logger.</param>
        public ProcessContractFromFeedExceptionService(
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ProcessContractFromFeedExceptionService> logger,
            IConfiguration configuration)
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task Process(ProcessContractFromFeedExceptionMessage message)
        {
            var displayText = $"{message.Ukprn}, contract {message.ContractNumber}-{message.ContractVersionNumber}, Ukprn {message.Ukprn}.";
            _logger.LogInformation($"Processing {nameof(ProcessContractFromFeedExceptionMessage)} for {displayText}.");

            string cdsUserExceptionEmail = _configuration.GetValue<string>("CdsUserExceptionEmail");

            if (string.IsNullOrEmpty(cdsUserExceptionEmail))
            {
                var contractDisplayText = $"{message.ContractNumber}-{message.ContractVersionNumber}";
                var errorMessage = string.Format(Constants.LogMessage, contractDisplayText, "Unable to find the email address from configuration->cdsUserExceptionEmail.");
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var personalisation = new Dictionary<string, object>()
            {
                 { "FeedStatus",  message.FeedStatus },
                 { "ExistingContractStatus",  message.ExistingContractStatus },
                 { "ContractNumber",  message.ContractNumber },
                 { "ParentContractNumber",  message.ParentContractNumber },
                 { "ContractVersionNumber",  message.ContractVersionNumber },
                 { "ContractTitle",  message.ContractTitle },
                 { "ParentFeedStatus",  message.ParentFeedStatus },
                 { "ExceptionTime",  message.ExceptionTime.DisplayFormat() },
                 { "ProviderName",  message.ProviderName },
                 { "Ukprn",  message.Ukprn }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(
                new List<string> { cdsUserExceptionEmail },
                Constants.RequestingService_Contracts,
                Constants.MessageType_ProcessContractFromFeedException,
                personalisation);

            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation($"Processed {nameof(ProcessContractFromFeedExceptionMessage)} for {displayText} and published to SharedEmailprocessorQueue.");
        }
    }
}
