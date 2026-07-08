using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations;
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

namespace Pds.Contracts.Notifications.Services.Implementations.SubcontractorDeclarations
{
    /// <summary>
    /// Subcontractor Declaration Email Service.
    /// </summary>
    public class SubcontractorDeclarationEmailService : SubcontractorDeclarationDataApiProvider, ISubcontractorDeclarationEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewPreviousSubcontractorDeclarations.ToString(),
            UserRole.SubmitSubcontractorDeclarations.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<SubcontractorDeclarationEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public SubcontractorDeclarationEmailService(
            IAuthenticationService<SubcontractorDeclarationDataApiConfiguration> authenticationService,
            IOptions<SubcontractorDeclarationDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<SubcontractorDeclarationEmailService> logger,
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
        public async Task Process(SubcontractorDeclarationEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(SubcontractorDeclarationEmailMessage)} with SubcontractorDeclarationId {message.SubcontractorDeclarationId}.");

            var fullSubcontractorDeclaration = await Get<FullSubcontractorDeclaration>($"{Constants.FullSubcontractorDeclarationGetByIdEndpoint}/{message.SubcontractorDeclarationId}") ?? throw new Exception("Error fetching full subcontractor declaration.");

            var audit = AuditHelper.ConstructAuditObject(
               string.Format(Constants.AuditMessage, nameof(SubcontractorDeclarationEmailMessage)),
               fullSubcontractorDeclaration.Ukprn,
               ActionType.EmailMessagePushed,
               Constants.ComponentName,
               SeverityLevel.Information);

            var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(fullSubcontractorDeclaration.Ukprn, _requiredRoles);

            if (userContactsResponse.Users.IsNotNullOrEmpty())
            {
                var toSubcontractorDeclarationRecipients = userContactsResponse.Users.Select(user => user.Email);

                var personalisation = new Dictionary<string, object>()
                {
                    { "SubmittedBy", fullSubcontractorDeclaration.SubmittedByDisplayName },
                    { "SubmittedOn", fullSubcontractorDeclaration.SubmittedAt.HasValue ? fullSubcontractorDeclaration.SubmittedAt.Value.DisplayFormat() : throw new Exception("Submitted Date is not avaialble.") }
                };

                var emailTemplateName = GetEmailTemplateName(fullSubcontractorDeclaration.SubcontractorDeclarationType);

                var notificationMessage = EmailNotificationHelper.ConstructNotification(toSubcontractorDeclarationRecipients, Constants.RequestingService_SubcontractorDeclarations, emailTemplateName, personalisation);

                await _notificationEmailQueueService.SendAsync(notificationMessage);

                _logger.LogInformation($"SubcontractorDeclaration id [{fullSubcontractorDeclaration.Id}], {audit.Message}");
            }
            else
            {
                var errorMessage = $"{nameof(SubcontractorDeclarationEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{fullSubcontractorDeclaration.Ukprn}]";
                _logger.LogError($"SubcontractorDeclaration id [{fullSubcontractorDeclaration.Id}], {errorMessage}");
                audit.UpdateErrorMessage(errorMessage);
            }

            await _auditService.AuditAsync(audit);
        }

        /// <summary>
        /// Message type for constructiong NotificationMessage object.
        /// </summary>
        /// <param name="subcontractorDeclarationType">Subcontractor declaration type.</param>
        /// <returns>Message type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid message type.</exception>
        private static string GetEmailTemplateName(SubcontractorDeclarationSubmissionType subcontractorDeclarationType)
        {
            return subcontractorDeclarationType switch
            {
                SubcontractorDeclarationSubmissionType.Full => Constants.MessageType_SubcontractorReturnSubmissionFullReturnEmail,
                SubcontractorDeclarationSubmissionType.Nil => Constants.MessageType_SubcontractorReturnSubmissionNilReturnEmail,
                _ => throw new ArgumentOutOfRangeException($"No email template found for expection type [{subcontractorDeclarationType}]")
            };
        }
    }
}
