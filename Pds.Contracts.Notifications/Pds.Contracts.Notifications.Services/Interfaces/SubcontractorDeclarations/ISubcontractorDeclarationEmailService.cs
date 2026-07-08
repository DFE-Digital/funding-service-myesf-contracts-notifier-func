using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations
{
    /// <summary>
    /// Subcontractor Declaration Email Service.
    /// </summary>
    public interface ISubcontractorDeclarationEmailService
    {
        /// <summary>
        /// Process and consume the subcontractor declaration email message.
        /// </summary>
        /// <param name="message">Subcontractor declaration email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(SubcontractorDeclarationEmailMessage message);
    }
}
