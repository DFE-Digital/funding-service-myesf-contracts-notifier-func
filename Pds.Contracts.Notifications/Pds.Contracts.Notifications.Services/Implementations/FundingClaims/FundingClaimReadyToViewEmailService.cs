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
    /// Funding Claim Ready To View Email Service.
    /// </summary>
    public class FundingClaimReadyToViewEmailService : FundingClaimsDataApiProvider, IFundingClaimReadyToViewEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewFundingClaimsAndReconciliationStatements.ToString(),
            UserRole.SignFundingClaims.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<FundingClaimReadyToViewEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimReadyToViewEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public FundingClaimReadyToViewEmailService(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<FundingClaimReadyToViewEmailService> logger,
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
        public async Task Process(FundingClaimReadyToViewEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(FundingClaimReadyToViewEmailMessage)} with FundingClaimId {message.FundingClaimId}.");

            var fundingClaim = await Get<FundingClaim>(FundingApiHelper.GetFundingClaimGetByIdUrl(message.FundingClaimId));

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(fundingClaim.Ukprn, _requiredRoles);

            var audit = AuditHelper.ConstructAuditObject(
              string.Format(Constants.AuditMessage, nameof(FundingClaimReadyToViewEmailMessage)),
              fundingClaim.Ukprn,
              ActionType.EmailMessagePushed,
              Constants.ComponentName,
              SeverityLevel.Information);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toFundingClaimRecipients = userContactsResponse.Users.Select(user => user.Email);

                var personalisation = new Dictionary<string, object>()
                {
                    { "FundingClaimNotification", fundingClaim.Title },
                    { "submitted date", fundingClaim.DateSubmitted.HasValue ? fundingClaim.DateSubmitted.Value.DisplayFormat() : throw new Exception("Submitted Date is not avaialble.") }
                };

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toFundingClaimRecipients, Constants.RequestingService_FundingClaims, Constants.MessageType_FundingClaimReadyToViewDateAvailableEmail, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation($"Funding id [{fundingClaim.Id}], {audit.Message}");
            }
            else
            {
                var errorMessage = $"{nameof(FundingClaimReadyToViewEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{fundingClaim.Ukprn}]";
                _logger.LogError($"Funding id [{fundingClaim.Id}], {errorMessage}");
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }
    }
}
