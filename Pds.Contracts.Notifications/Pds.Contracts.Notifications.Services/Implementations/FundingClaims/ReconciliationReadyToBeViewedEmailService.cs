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
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.FundingClaims
{
    /// <summary>
    /// Reconciliation Ready To Be Viewed Email Service.
    /// </summary>
    public class ReconciliationReadyToBeViewedEmailService : FundingClaimsDataApiProvider, IReconciliationReadyToBeViewedEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(),
            UserRole.SignFundingClaims.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ReconciliationReadyToBeViewedEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationReadyToBeViewedEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ReconciliationReadyToBeViewedEmailService(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ReconciliationReadyToBeViewedEmailService> logger,
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
        public async Task Process(ReconciliationReadyToBeViewedEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ReconciliationReadyToBeViewedEmailMessage)} with ReconciliationId {message.ReconciliationId}.");

            var reconciliation = await Get<Reconciliation>($"{Constants.ReconciliationGetByIdEndpoint}/{message.ReconciliationId}") ?? throw new Exception("Error fetching reconciliation.");

            var audit = AuditHelper.ConstructAuditObject(
               string.Format(Constants.AuditMessage, nameof(ReconciliationReadyToBeViewedEmailMessage)),
               reconciliation.Ukprn,
               ActionType.EmailMessagePushed,
               Constants.ComponentName,
               SeverityLevel.Information);

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(reconciliation.Ukprn, _requiredRoles);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toFundingClaimReconciliationRecipients = userContactsResponse.Users.Select(user => user.Email);

                var personalisation = new Dictionary<string, object>()
                {
                    { "ReconciliationTitle",  reconciliation.Title },
                    { "ReconciliationType", reconciliation.Type.GetPropertyValue<ReconciliationType, DisplayAttribute>(o => o.Name) }
                };

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toFundingClaimReconciliationRecipients, Constants.RequestingService_FundingClaims, Constants.MessageType_ReconciliationReadyToBeViewedEmail, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation($"Reconciliation id [{reconciliation.Id}], {audit.Message}");
            }
            else
            {
                var errorMessage = $"{nameof(ReconciliationReadyToBeViewedEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{reconciliation.Ukprn}].";
                _logger.LogError($"Reconciliation id [{reconciliation.Id}], {errorMessage}");
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }
    }
}
