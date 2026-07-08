using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Azure.Interfaces.Messaging;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Contract reminder service.
    /// </summary>
    public class ContractNotificationService : BaseApiClient<ContractsDataApiConfiguration>, IContractNotificationService
    {
        /// <summary>
        /// The audit user.
        /// </summary>
        public const string Audit_User_System = "System-Notifier";

        private readonly ILoggerAdapter<ContractNotificationService> _logger;

        private readonly IAuditService _auditService;

        private readonly ContractsDataApiConfiguration _dataApiConfiguration;

        private readonly IAzureMessagingServiceBusService _azureMessagingServiceBusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotificationService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="azureMessagingServiceBusService">The messaging service to use for queuing reminders.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        /// <param name="auditService">The audit service.</param>
        public ContractNotificationService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            IAzureMessagingServiceBusService azureMessagingServiceBusService,
            ILoggerAdapter<ContractNotificationService> logger,
            IAuditService auditService)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _logger = logger;
            _azureMessagingServiceBusService = azureMessagingServiceBusService;
            _auditService = auditService;
            _dataApiConfiguration = configurationOptions.Value;
        }

        /// <inheritdoc/>
        public async Task<ContractReminders> GetOverdueContracts()
        {
            string querystring = CreateQueryString(_dataApiConfiguration.ContractReminderQuerystring);
            _logger.LogInformation($"Requesting a list of contracts with overdue reminders with querystring {querystring}.");
            return await Get<ContractReminders>(querystring);
        }

        /// <inheritdoc/>
        public async Task QueueContractEmailReminderMessage(Contract contract)
        {
            _logger.LogInformation($"Queuing email reminder for contract [{contract.ContractNumber}].");

            var message = new ContractReminderMessage() { ContractId = contract.Id };

            await _azureMessagingServiceBusService.SendMessageAsync(message, Constants.ContractReminderEmailQueue);

            await _auditService.AuditAsync(
                new Pds.Audit.Api.Client.Models.Audit
                {
                    Severity = 0,
                    Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                    Ukprn = contract.Ukprn,
                    Message = $"ContractReminderMessage has been queued for contract with Id [{contract.Id}].",
                    User = Audit_User_System
                });
        }

        /// <inheritdoc/>
        public async Task NotifyContractReminderSent(Contract contractChange)
        {
            var updateRequest = new ContractUpdateRequest
            {
                Id = contractChange.Id,
                ContractNumber = contractChange.ContractNumber,
                ContractVersion = contractChange.ContractVersion
            };

            _logger.LogInformation($"Updating the last reminder date on contract with id [{contractChange.ContractNumber}].");
            await Patch(Constants.ContractReminderPatchEndpoint, updateRequest);

            await _auditService.AuditAsync(
                new Pds.Audit.Api.Client.Models.Audit
                {
                    Severity = 0,
                    Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                    Ukprn = contractChange.Ukprn,
                    Message = $"Updated 'Last email reminder sent date' for contract with Id {contractChange.Id}",
                    User = Audit_User_System
                });
        }

        /// <inheritdoc/>
        protected override Action<ApiGeneralException> FailureAction
            => exception =>
            {
                _logger.LogError(exception, exception.Message);
                throw exception;
            };

        #region Helper Functions

        private static string CreateQueryString(QuerystringEndpointConfiguration endpointConfiguration)
        {
            string querystring = $"{Constants.ContractReminderEndpoint}?";
            IList<string> query = new List<string>();

            foreach (var item in endpointConfiguration.QueryParameters.Keys)
            {
                query.Add($"{item}={endpointConfiguration.QueryParameters[item]}");
            }

            querystring += string.Join('&', query);
            return querystring;
        }

        #endregion
    }
}