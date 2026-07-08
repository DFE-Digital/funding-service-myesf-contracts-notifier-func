using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Contracts Query Email Service.
    /// </summary>
    public class ContractsQueryEmailService : ContractsDataApiProvider, IContractsQueryEmailService
    {
        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ContractsQueryEmailService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsQueryEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractsQueryEmailService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ContractsQueryEmailService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(ContractsQueryEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractsQueryEmailMessage)} for Contract Id {message.ContractId}.");

            var contract = await Get<Contract>(ContractApiHelper.GetContractByIdUrl(message.ContractId));

            string serviceNowEmailAddress = _configuration.GetValue<string>("ServiceNowEmailAddress");

            if (string.IsNullOrEmpty(serviceNowEmailAddress))
            {
                var errorMessage = string.Format(Constants.LogMessage, contract.ContractDisplayText, "Unable to find the email address from configuration->ServiceNowEmailAddress.");
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var personalisation = new Dictionary<string, object>()
            {
                 { "ProviderUserName",  message.ProviderUserName },
                 { "ProviderEmailAddress",  message.ProviderEmailAddress },
                 { "ProviderName",  message.ProviderName },
                 { "ProviderUkprn",  contract.Ukprn },
                 { "ContractTitle",  contract.Title },
                 { "ContractDocumentName",  contract.ContractContent.FileName },
                 { "QueryReason",  message.QueryReason },
                 { "QueryDetail",  message.QueryDetail }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(
                new List<string> { serviceNowEmailAddress },
                Constants.RequestingService_Contracts,
                Constants.MessageType_ContractsQueryEmail,
                personalisation);

            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation($"Processed {nameof(ContractsQueryEmailMessage)} for Contract Id {message.ContractId}.");
        }
    }
}
