using Microsoft.Azure.Functions.Worker;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Process Contract From Feed Exception Function which listens and process the processcontractfromfeedexception queue messages.
    /// </summary>
    public class ProcessContractFromFeedExceptionFunction
    {
        private readonly IProcessContractFromFeedExceptionService _processContractFromFeedExceptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContractFromFeedExceptionFunction"/> class.
        /// </summary>
        /// <param name="processContractFromFeedExceptionService">Process Contract From Feed Exception Service.</param>
        public ProcessContractFromFeedExceptionFunction(IProcessContractFromFeedExceptionService processContractFromFeedExceptionService)
        {
            _processContractFromFeedExceptionService = processContractFromFeedExceptionService;
        }

        /// <summary>
        /// Listens to processcontractfromfeedexception queue and send the email.
        /// </summary>
        /// <param name="processContractFromFeedExceptionMessage">Process contract from feed exception from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [Function(nameof(ProcessContractFromFeedExceptionFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ProcessContractFromFeedExceptionQueue, Connection = "AzureMessagingServiceBusOptions:ConnectionString")] ProcessContractFromFeedExceptionMessage processContractFromFeedExceptionMessage)
        {
            try
            {
                ValidateServiceBusMessage(processContractFromFeedExceptionMessage);

                await this._processContractFromFeedExceptionService.Process(processContractFromFeedExceptionMessage);
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateServiceBusMessage(ProcessContractFromFeedExceptionMessage message)
        {
            It.IsEmpty(message.ParentFeedStatus)
            .AsGuard<ArgumentNullException>();

            It.IsEmpty(message.FeedStatus)
            .AsGuard<ArgumentNullException>();

            It.IsEmpty(message.ExistingContractStatus)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.ParentContractNumber)
            .AsGuard<ArgumentException>();

            It.IsEmpty(message.ContractNumber)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.ContractVersionNumber)
            .AsGuard<ArgumentException>();

            It.IsEmpty(message.ContractTitle)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.ExceptionTime)
            .AsGuard<ArgumentException>();

            It.IsEmpty(message.ProviderName)
            .AsGuard<ArgumentNullException>();

            It.IsNullOrDefault(message.Ukprn)
            .AsGuard<ArgumentException>();
        }
    }
}
