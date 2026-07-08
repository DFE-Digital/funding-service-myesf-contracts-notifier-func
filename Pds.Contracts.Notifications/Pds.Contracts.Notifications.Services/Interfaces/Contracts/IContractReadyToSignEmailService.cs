using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Ready To Sign Email Service.
    /// </summary>
    public interface IContractReadyToSignEmailService
    {
        /// <summary>
        /// Process and consume the contract ready to sign email message.
        /// </summary>
        /// <param name="message">Contract ready to sign email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractReadyToSignEmailMessage message);
    }
}
