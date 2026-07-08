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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Contract Withdrawn Email Service.
    /// </summary>
    public class ContractWithdrawnEmailService : ContractsDataApiProvider, IContractWithdrawnEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewContractsAndAgreements.ToString(),
            UserRole.SignContractsAndAgreements.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ContractWithdrawnEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractWithdrawnEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractWithdrawnEmailService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ContractWithdrawnEmailService> logger,
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
        public async Task Process(ContractWithdrawnEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractWithdrawnEmailMessage)} with Ukprn {message.Ukprn}, contract number {message.ContractNumber} and version number {message.VersionNumber}.");

            var contract = await Get<Contract>(ContractApiHelper.CreateContractQueryString(message.ContractNumber, message.VersionNumber, message.Ukprn));

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(message.Ukprn, _requiredRoles);

            var audit = AuditHelper.ConstructAuditObject(
                    string.Format(Constants.AuditMessage, nameof(ContractWithdrawnEmailMessage)),
                    contract.Ukprn,
                    ActionType.EmailMessagePushed,
                    Constants.ComponentName,
                    SeverityLevel.Information);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toContractRecipients = userContactsResponse.Users.Select(user => user.Email);

                var personalisation = new Dictionary<string, object>()
                {
                    { "ContractFriendlyName",  contract.Title },
                    { "contract or agreement",  contract.DocumentType }
                };

                var emailTemplateName = GetEmailTemplateName(contract.Status);

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toContractRecipients, Constants.RequestingService_Contracts, emailTemplateName, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation(string.Format(Constants.LogMessage, contract.ContractDisplayText, audit.Message));
            }
            else
            {
                var errorMessage = $"{nameof(ContractWithdrawnEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{contract.Ukprn}].";
                _logger.LogError(string.Format(Constants.LogMessage, contract.ContractDisplayText, errorMessage));
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }

        /// <summary>
        /// Message type for constructiong NotificationMessage object.
        /// </summary>
        /// <param name="contractStatus">Contract Status.</param>
        /// <returns>Message type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid message type.</exception>
        private static string GetEmailTemplateName(ContractStatus contractStatus)
        {
            return contractStatus switch
            {
                ContractStatus.WithdrawnByProvider => Constants.MessageType_ContractWithdrawnByProviderEmail,
                ContractStatus.WithdrawnByAgency => Constants.MessageType_ContractWithdrawnByESFAEmail,
                _ => throw new ArgumentOutOfRangeException($"No email template found for contract Status [{contractStatus.ToString()}]")
            };
        }
    }
}
