using Pds.Core.ApiClient;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Configuration setting for the Contracts Data API.
    /// </summary>
    public class ContractsDataApiConfiguration : BaseApiClientConfiguration
    {
        /// <summary>
        /// Gets or sets the contact reminder query configuration settings.
        /// </summary>
        public QuerystringEndpointConfiguration ContractReminderQuerystring { get; set; }
    }
}
