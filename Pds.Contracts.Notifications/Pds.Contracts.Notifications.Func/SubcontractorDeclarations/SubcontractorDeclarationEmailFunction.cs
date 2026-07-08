using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.SubcontractorDeclarations
{
    /// <summary>
    /// Subcontractor declaration email function which listens and process the subcontractordeclarationemail queue messages.
    /// </summary>
    public class SubcontractorDeclarationEmailFunction
    {
        private readonly ISubcontractorDeclarationEmailService _subcontractorDeclarationEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationEmailFunction"/> class.
        /// </summary>
        /// <param name="subcontractorDeclarationEmailService">Subcontractor declaration email service.</param>
        public SubcontractorDeclarationEmailFunction(ISubcontractorDeclarationEmailService subcontractorDeclarationEmailService)
        {
            _subcontractorDeclarationEmailService = subcontractorDeclarationEmailService;
        }

        /// <summary>
        /// Listens to subcontractordeclarationemail queue and send the email.
        /// </summary>
        /// <param name="subcontractorDeclarationEmailMessage">Subcontractor declaration email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(SubcontractorDeclarationEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.SubcontractorDeclarationEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] SubcontractorDeclarationEmailMessage subcontractorDeclarationEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(subcontractorDeclarationEmailMessage.SubcontractorDeclarationId)
                    .AsGuard<ArgumentException>();

                await _subcontractorDeclarationEmailService.Process(subcontractorDeclarationEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
