using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contracts Query Email.
    /// </summary>
    public interface IContractsQueryEmailService
    {
        /// <summary>
        /// Process and consume the contracts query email message.
        /// </summary>
        /// <param name="message">contracts query email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractsQueryEmailMessage message);
    }
}
