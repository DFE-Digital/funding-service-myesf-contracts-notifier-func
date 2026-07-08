using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;

namespace Pds.Contracts.Notifications.Func.Contracts.Lifecycle
{
    /// <summary>
    /// Example timer triggered Azure Function.
    /// </summary>
    public class ContractsReminderTimerFunction
    {
        private readonly IContractReminderProcessingService _contractReminderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsReminderTimerFunction"/> class.
        /// </summary>
        /// <param name="contractReminderService">The example service.</param>
        public ContractsReminderTimerFunction(IContractReminderProcessingService contractReminderService)
        {
            _contractReminderService = contractReminderService;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="timer">The timer info.</param>
        /// <param name="log">The logger.</param>
        /// <returns>Async task.</returns>
        [Function("ContractsReminderTimerFunction")]
        public async Task Run(
            [TimerTrigger("0 0 8 * * *")] TimerInfo timer,
            ILogger log)
        {
            log?.LogInformation($"[Start] Timer trigger contracts reminder function started execution at: {DateTime.Now}.");

            await _contractReminderService.IssueContractReminders();

            log?.LogInformation($"[End] Timer trigger contracts reminder function completed execution at: {DateTime.Now}.");
        }
    }
}