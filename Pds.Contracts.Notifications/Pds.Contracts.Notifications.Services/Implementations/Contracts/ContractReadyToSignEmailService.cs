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
using Pds.Core.DfESignIn.Extensions;
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
    /// Contract Ready To Sign Email Service.
    /// </summary>
    public class ContractReadyToSignEmailService : ContractsDataApiProvider, IContractReadyToSignEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewContractsAndAgreements.ToString(),
            UserRole.SignContractsAndAgreements.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ContractReadyToSignEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        private Contract ContractData { get; set; }

        private Audit.Api.Client.Models.Audit Audit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToSignEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractReadyToSignEmailService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ContractReadyToSignEmailService> logger,
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
        public async Task Process(ContractReadyToSignEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractReadyToSignEmailMessage)} with Ukprn {message.Ukprn}, contract number {message.ContractNumber} and version number {message.VersionNumber}.");

            ContractData = await Get<Contract>(ContractApiHelper.CreateContractQueryString(message.ContractNumber, message.VersionNumber, message.Ukprn));

            if (ContractData.Status == ContractStatus.PublishedToProvider && ContractData.AmendmentType != ContractAmendmentType.Notfication)
            {
                var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(message.Ukprn, _requiredRoles);

                var recipients_toSign = userContactsResponse.EmailAddressOf(UserRole.SignContractsAndAgreements);

                var recipients_toViewOnly = userContactsResponse.EmailAddressOf(UserRole.ViewContractsAndAgreements)?.Except(recipients_toSign);

                var auditresult = await PublishEmailMessageToSendEmail(recipients_toSign, Constants.MessageType_ContractReadyToSignEmail, UserRole.SignContractsAndAgreements.ToString());

                await _auditService.AuditAsync(Audit);

                auditresult = await PublishEmailMessageToSendEmail(recipients_toViewOnly, Constants.MessageType_ContractReadyToSignViewOnlyEmail, UserRole.ViewContractsAndAgreements.ToString());

                if (auditresult)
                {
                    await _auditService.AuditAsync(Audit);
                }
            }
            else
            {
                _logger.LogError($"Contract id {ContractData.ContractDisplayText} doesn't meet the the criteria - Status [{ContractData.Status}], Amendmant type [{ContractData.AmendmentType}]. {nameof(ContractReadyToSignEmailMessage)} was not processed.");
            }
        }

        /// <summary>
        /// publish a message to email processor.
        /// </summary>
        /// <param name="toContractRecipients">toContractRecipients.</param>
        /// <param name="emailTemplateName">Email Templat eName.</param>
        /// <param name="roleName">roleName.</param>
        /// <returns>Audit data.</returns>
        private async Task<bool> PublishEmailMessageToSendEmail(IEnumerable<string> toContractRecipients, string emailTemplateName, string roleName)
        {
            Audit = AuditHelper.ConstructAuditObject(
                string.Format(Constants.AuditMessage, $"{nameof(ContractReadyToSignEmailMessage)}{(roleName == UserRole.SignContractsAndAgreements.ToString() ? string.Empty : " view only")}"),
                ContractData.Ukprn,
                ActionType.EmailMessagePushed,
                Constants.ComponentName,
                SeverityLevel.Information);

            if (!toContractRecipients.IsNotNullOrEmpty())
            {
                var errorMessage = $"{nameof(ContractReadyToSignEmailMessage)} processed and no users found with roles [{roleName}] for organisation [{ContractData.Ukprn}]";
                if (roleName == UserRole.SignContractsAndAgreements.ToString())
                {
                    _logger.LogError(string.Format(Constants.LogMessage, ContractData.ContractDisplayText, errorMessage));
                    Audit.UpdateErrorMessage(errorMessage);
                }
                else
                {
                    _logger.LogInformation(string.Format(Constants.LogMessage, ContractData.ContractDisplayText, errorMessage));
                }

                return roleName == UserRole.SignContractsAndAgreements.ToString() ? true : false;
            }

            var personalisation = new Dictionary<string, object>()
            {
                { "DocumentTitle",  ContractData.Title },
                { "contract or agreement",  ContractData.DocumentType }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(toContractRecipients, Constants.RequestingService_Contracts, emailTemplateName, personalisation);

            var test = notificationMessage.EmailAddresses;
            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation(string.Format(Constants.LogMessage, ContractData.ContractDisplayText, Audit.Message));

            return true;
        }
    }
}
