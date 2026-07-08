using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.FundingClaims
{
    /// <summary>
    /// Funding Claim Ready To Sign Email Service.
    /// </summary>
    public class FundingClaimReadyToSignEmailService : FundingClaimsDataApiProvider, IFundingClaimReadyToSignEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(),
            UserRole.SignFundingClaims.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FundingClaimReadyToSignEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        private FundingClaim FundingClaimData { get; set; }

        private Audit.Api.Client.Models.Audit Audit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimReadyToSignEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public FundingClaimReadyToSignEmailService(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<FundingClaimReadyToSignEmailService> logger,
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
        public async Task Process(FundingClaimReadyToSignEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FundingClaimReadyToSignEmailMessage)} with FundingClaimId [{message.FundingClaimId}].");

            FundingClaimData = await Get<FundingClaim>(FundingApiHelper.GetFundingClaimGetByIdUrl(message.FundingClaimId));

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(FundingClaimData.Ukprn, _requiredRoles);

            var recipients_toSign = userContactsResponse.EmailAddressOf(UserRole.SignFundingClaims);

            var recipients_toViewOnly = userContactsResponse.EmailAddressOf(UserRole.ViewFundingClaimsAndReconciliationStatements)?.Except(recipients_toSign);

            var auditresult = await PublishEmailMessageToSendEmail(recipients_toSign, Constants.MessageType_FundingClaimReadyToSignEmail, UserRole.SignFundingClaims.ToString());

            await _auditService.AuditAsync(Audit);

            auditresult = await PublishEmailMessageToSendEmail(recipients_toViewOnly, Constants.MessageType_FundingClaimReadyToSignViewOnlyEmail, UserRole.ViewFundingClaimsAndReconciliationStatements.ToString());

            if (auditresult)
            {
                await _auditService.AuditAsync(Audit);
            }
        }

        private async Task<bool> PublishEmailMessageToSendEmail(IEnumerable<string> toFundingClaimRecipients, string emailTemplateName, string roleName)
        {
            Audit = AuditHelper.ConstructAuditObject(
               string.Format(Constants.AuditMessage, $"{nameof(FundingClaimReadyToSignEmailMessage)}{(roleName == UserRole.SignFundingClaims.ToString() ? string.Empty : " view only")}"),
               FundingClaimData.Ukprn,
               ActionType.EmailMessagePushed,
               Constants.ComponentName,
               SeverityLevel.Information);

            if (!toFundingClaimRecipients.IsNotNullOrEmpty())
            {
                var errorMessage = $"{nameof(FundingClaimReadyToSignEmailMessage)} processed and no users found with roles [{roleName}] for organisation [{FundingClaimData.Ukprn}]";
                if (roleName == UserRole.SignFundingClaims.ToString())
                {
                    _logger.LogError($"Funding id [{FundingClaimData.Id}], {errorMessage}");
                    Audit.UpdateErrorMessage(errorMessage);
                }
                else
                {
                    _logger.LogInformation($"Funding id [{FundingClaimData.Id}], {errorMessage}");
                }

                return roleName == UserRole.SignFundingClaims.ToString() ? true : false;
            }

            var personalisation = new Dictionary<string, object>()
            {
                { "FundingClaimNotification", FundingClaimData.Title },
                { "DueDate", FundingClaimData.FundingClaimWindow.SignatureCloseDate.HasValue ? FundingClaimData.FundingClaimWindow.SignatureCloseDate.Value.DisplayFormat() : throw new Exception("Due Date is not avaialble.") }
            };

            var notificationMessage = EmailNotificationHelper.ConstructNotification(toFundingClaimRecipients, Constants.RequestingService_FundingClaims, emailTemplateName, personalisation);

            await _notificationEmailQueueService.SendAsync(notificationMessage);

            _logger.LogInformation($"Funding id [{FundingClaimData.Id}], {Audit.Message}");

            return true;
        }
    }
}
