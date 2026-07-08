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
    /// Funding Claim Signed Email Service.
    /// </summary>
    public class FundingClaimSignedEmailService : FundingClaimsDataApiProvider, IFundingClaimSignedEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(),
            UserRole.SignFundingClaims.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FundingClaimSignedEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimSignedEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public FundingClaimSignedEmailService(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<FundingClaimSignedEmailService> logger,
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
        public async Task Process(FundingClaimSignedEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FundingClaimSignedEmailMessage)} with FundingClaimId {message.FundingClaimId}.");

            var fundingClaim = await Get<FundingClaim>(FundingApiHelper.GetFundingClaimGetByIdUrl(message.FundingClaimId));

            var audit = AuditHelper.ConstructAuditObject(
              string.Format(Constants.AuditMessage, nameof(FundingClaimSignedEmailMessage)),
              fundingClaim.Ukprn,
              ActionType.EmailMessagePushed,
              Constants.ComponentName,
              SeverityLevel.Information);

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(fundingClaim.Ukprn, _requiredRoles);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toFundingClaimRecipients = userContactsResponse.Users.Select(user => user.Email);

                var personalisation = new Dictionary<string, object>()
                {
                    { "FundingClaimTitle",  fundingClaim.Title },
                    { "SignedByDisplayName", fundingClaim.SignedByDisplayName },
                    { "SignedOn", fundingClaim.SignedOn.HasValue ? fundingClaim.SignedOn.Value.DisplayFormat() : throw new Exception("SignedOn value is not avaialble.") }
                };

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toFundingClaimRecipients, Constants.RequestingService_FundingClaims, Constants.MessageType_FundingClaimSignedEmail, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation($"Funding id [{fundingClaim.Id}], {audit.Message}");
            }
            else
            {
                var errorMessage = $"{nameof(FundingClaimSignedEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{fundingClaim.Ukprn}]";
                _logger.LogError($"Funding id [{message.FundingClaimId}], {errorMessage}");
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }
    }
}
