using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Interface for contract status change publisher.
    /// </summary>
    public interface IContractStatusChangePublisher
    {
        /// <summary>
        /// Notify the contract has been withdrawn.
        /// </summary>
        /// <param name="contract">The contract that has changed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task NotifyContractWithdrawnAsync(Contract contract);

        /// <summary>
        /// Notify the contract has been approved.
        /// </summary>
        /// <param name="contract">The contract that has changed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task NotifyContractApprovedAsync(Contract contract);

        /// <summary>
        /// Notify the contract is ready to sign.
        /// </summary>
        /// <param name="contract">The contract that has changed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task NotifyContractIsReadyToSignAsync(Contract contract);

        /// <summary>
        /// Notify the contract changes are ready for review.
        /// </summary>
        /// <param name="contract">The contract that has changed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task NotifyContractChangesAreReadyForReviewAsync(Contract contract);
    }
}
