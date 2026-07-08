using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Common.Identity.Enums;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Contract Reminder Email Service.
    /// </summary>
    public class ContractReminderEmailService : ContractsDataApiProvider, IContractReminderEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewContractsAndAgreements.ToString(),
            UserRole.SignContractsAndAgreements.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ContractReminderEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReminderEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractReminderEmailService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ContractReminderEmailService> logger,
            IDfESignInPublicApi dfESignInPublicApi,
            IAuditService auditService,
            HttpClient httpClient)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _dfESignInPublicApi = dfESignInPublicApi;
            _auditService = auditService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(ContractReminderEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractReminderEmailMessage)} with ContractId {message.ContractId}.");

            var contract = await Get<Contract>(ContractApiHelper.GetContractByIdUrl(message.ContractId));

            if (contract.Status == ContractStatus.PublishedToProvider)
            {
                var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(contract.Ukprn, _requiredRoles);

                var audit = AuditHelper.ConstructAuditObject(
                       string.Format(Constants.AuditMessage, nameof(ContractReminderEmailMessage)),
                       contract.Ukprn,
                       ActionType.EmailMessagePushed,
                       Constants.ComponentName,
                       SeverityLevel.Information);

                if (userContactsResponse.Users.IsNotNullOrEmpty())
                {
                    var toContractRecipients = userContactsResponse.Users.Select(user => user.Email);

                    var personalisation = new Dictionary<string, object>()
                    {
                         { "DocumentTitle",  contract.Title },
                         { "contract or agreement",  contract.DocumentType }
                    };

                    var notificationMessage = EmailNotificationHelper.ConstructNotification(toContractRecipients, Constants.RequestingService_Contracts, Constants.MessageType_ContractReminderEmail, personalisation);

                    await _notificationEmailQueueService.SendAsync(notificationMessage);

                    _logger.LogInformation(string.Format(Constants.LogMessage, contract.ContractDisplayText, audit.Message));
                }
                else
                {
                    var errorMessage = $"{nameof(ContractReminderEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{contract.Ukprn}].";

                    _logger.LogError(string.Format(Constants.LogMessage, contract.ContractDisplayText, errorMessage));

                    audit.UpdateErrorMessage(errorMessage);
                }

                await _auditService.AuditAsync(audit);
            }
            else
            {
                var infoMessage = $"{nameof(ContractReminderEmailMessage)} processed and contract status is not published to provider, Status: [{contract.Status}].";

                _logger.LogInformation(string.Format(Constants.LogMessage, contract.ContractDisplayText, infoMessage));
            }
        }
    }
}
