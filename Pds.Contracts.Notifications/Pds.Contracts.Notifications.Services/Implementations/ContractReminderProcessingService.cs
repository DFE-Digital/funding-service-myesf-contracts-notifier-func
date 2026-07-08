using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractReminderProcessingService : IContractReminderProcessingService
    {
        /// <summary>
        /// The audit user.
        /// </summary>
        public const string Audit_User_System = "System-Notifier";

        private readonly IContractNotificationService _contractNotificationService;
        private readonly ILoggerAdapter<ContractReminderProcessingService> _logger;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReminderProcessingService"/> class.
        /// </summary>
        /// <param name="contractNotificationService">Contract notification service to use to access contracts data.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        /// <param name="auditService">The audit service.</param>
        public ContractReminderProcessingService(
            IContractNotificationService contractNotificationService,
            ILoggerAdapter<ContractReminderProcessingService> logger,
            IAuditService auditService)
        {
            _contractNotificationService = contractNotificationService;
            _logger = logger;
            _auditService = auditService;
        }

        /// <inheritdoc/>
        public async Task IssueContractReminders()
        {
            await _auditService.TrySendAuditAsync(
               new Audit.Api.Client.Models.Audit
               {
                   Severity = 0,
                   Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                   Ukprn = null,
                   Message = $"Contract Reminder function has been triggered.",
                   User = Audit_User_System
               });

            ContractReminders reminders = null;
            int remindersSent = 0;
            do
            {
                reminders = await _contractNotificationService.GetOverdueContracts();
                if (reminders?.Contracts?.Count > 0)
                {
                    _logger.LogInformation($"Processing a list of contracts with {reminders.Contracts.Count} records.");
                    IList<Task> tasks = new List<Task>();
                    foreach (var contract in reminders.Contracts)
                    {
                        var reminderProcess = Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInformation($"Starting processing contract with id {contract.Id}.");

                                await _contractNotificationService.QueueContractEmailReminderMessage(contract);

                                await _contractNotificationService.NotifyContractReminderSent(contract);

                                _logger.LogInformation($"Completed processing for contract with id {contract.Id}.");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Contract with id [{contract.Id}] failed to process successfully.");
                                throw;
                            }
                        });

                        tasks.Add(reminderProcess);
                    }

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error processign one or more records.");
                    }

                    if (tasks.All(p => !p.IsCompletedSuccessfully))
                    {
                        _logger.LogCritical("All processes failed to complete successfully.  Aborting...");
                        _logger.LogInformation($"Contract reminders processes aborted.  Sent {remindersSent} reminders prior to abort.");
                        throw new ContractProcessingException("All processes failed to complete successfully.", tasks.First(p => !p.IsCompletedSuccessfully).Exception);
                    }
                    else
                    {
                        remindersSent += tasks.Count(p => p.IsCompletedSuccessfully);
                    }
                }
            }
            while (reminders?.Contracts?.Count > 0);
            {
                await _auditService.TrySendAuditAsync(
                   new Audit.Api.Client.Models.Audit
                   {
                       Severity = 0,
                       Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                       Ukprn = null,
                       Message = $"Contract Reminder function has completed. {remindersSent} contract email reminder(s) processed.",
                       User = Audit_User_System
                   });
                _logger.LogInformation($"Contract reminders process completed. Sent {remindersSent} reminders.");
            }
        }
    }
}