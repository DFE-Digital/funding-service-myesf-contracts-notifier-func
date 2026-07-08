using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.FundingClaims
{
    /// <summary>
    /// Reconciliation ready to be viewed function which listens and process the reconciliationreadytobeviewedemail queue messages.
    /// </summary>
    public class ReconciliationReadyToBeViewedEmailFunction
    {
        private readonly IReconciliationReadyToBeViewedEmailService _reconciliationReadyToBeViewedEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationReadyToBeViewedEmailFunction"/> class.
        /// </summary>
        /// <param name="reconciliationReadyToBeViewedEmailService">Reconciliation ready to be viewed email service.</param>
        public ReconciliationReadyToBeViewedEmailFunction(IReconciliationReadyToBeViewedEmailService reconciliationReadyToBeViewedEmailService)
        {
            _reconciliationReadyToBeViewedEmailService = reconciliationReadyToBeViewedEmailService;
        }

        /// <summary>
        /// Listens to reconciliationreadytobeviewedemail queue and send the email.
        /// </summary>
        /// <param name="reconciliationReadyToBeViewedEmailMessage">Reconciliation ready to be viewed email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ReconciliationReadyToBeViewedEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ReconciliationReadyToBeViewedEmailQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ReconciliationReadyToBeViewedEmailMessage reconciliationReadyToBeViewedEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(reconciliationReadyToBeViewedEmailMessage.ReconciliationId)
                    .AsGuard<ArgumentException>();

                await _reconciliationReadyToBeViewedEmailService.Process(reconciliationReadyToBeViewedEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
