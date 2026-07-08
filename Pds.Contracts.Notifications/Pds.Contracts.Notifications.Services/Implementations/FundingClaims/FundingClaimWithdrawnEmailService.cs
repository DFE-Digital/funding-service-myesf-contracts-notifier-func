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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.FundingClaims
{
    /// <summary>
    /// Funding Claim Withdrawn Email Service.
    /// </summary>
    public class FundingClaimWithdrawnEmailService : FundingClaimsDataApiProvider, IFundingClaimWithdrawnEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(),
            UserRole.SignFundingClaims.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FundingClaimWithdrawnEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimWithdrawnEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public FundingClaimWithdrawnEmailService(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<FundingClaimWithdrawnEmailService> logger,
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
        public async Task Process(FundingClaimWithdrawnEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FundingClaimWithdrawnEmailMessage)} with FundingClaimId {message.FundingClaimId}.");

            var currentFundingClaim = await Get<FundingClaim>(FundingApiHelper.GetFundingClaimGetByIdUrl(message.FundingClaimId));

            var audit = AuditHelper.ConstructAuditObject(
                string.Format(Constants.AuditMessage, nameof(FundingClaimWithdrawnEmailMessage)),
                currentFundingClaim.Ukprn,
                ActionType.EmailMessagePushed,
                Constants.ComponentName,
                SeverityLevel.Information);

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(currentFundingClaim.Ukprn, _requiredRoles);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toFundingClaimRecipients = userContactsResponse.Users.Select(user => user.Email);

                var previouslySignedFundingClaim = await Get<FundingClaim>($"{Constants.PreviouslySignedVersionOfFundingClaimByIdEndpoint}/{currentFundingClaim.Id}");

                var personalisation = new Dictionary<string, object>()
                {
                    { "FundingClaimTitle", currentFundingClaim.Title },
                    {
                        "Deadline", currentFundingClaim.FundingClaimWindow.SignatureCloseDate.HasValue ?
                        currentFundingClaim.FundingClaimWindow.SignatureCloseDate.Value.DisplayFormat() :
                        throw new Exception("SignatureCloseDate value is not avaialble.")
                    }
                };

                string messageType = previouslySignedFundingClaim == null ? Constants.MessageType_FundingClaimWithdrawnNotSignedEmail.ToString() : Constants.MessageType_FundingClaimWithdrawnPreviousVersionSignedEmail;

                if (previouslySignedFundingClaim != null)
                {
                    personalisation.Add("PreviousSigner", previouslySignedFundingClaim.SignedByDisplayName);
                    personalisation.Add("PreviousSignedDateTime", previouslySignedFundingClaim.SignedOn.HasValue ? previouslySignedFundingClaim.SignedOn.Value.DisplayFormat() : throw new Exception("Previously SignedOn date value is not avaialble."));
                }

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toFundingClaimRecipients, Constants.RequestingService_FundingClaims, messageType, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation($"Funding id [{currentFundingClaim.Id}], {audit.Message}");
            }
            else
            {
                var errorMessage = $"{nameof(FundingClaimWithdrawnEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{currentFundingClaim.Ukprn}]";
                _logger.LogError($"Funding id [{currentFundingClaim.Id}], {errorMessage}");
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }
    }
}
