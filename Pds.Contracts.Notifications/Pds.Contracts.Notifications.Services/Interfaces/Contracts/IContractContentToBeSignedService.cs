using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.Contracts
{
    /// <summary>
    /// Contract Content To Be Signed Service.
    /// </summary>
    public interface IContractContentToBeSignedService
    {
        /// <summary>
        /// Consume and process the contract content to be signed message.
        /// </summary>
        /// <param name="message">Contract content to be signed message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(ContractContentToBeSignedMessage message);
    }
}
