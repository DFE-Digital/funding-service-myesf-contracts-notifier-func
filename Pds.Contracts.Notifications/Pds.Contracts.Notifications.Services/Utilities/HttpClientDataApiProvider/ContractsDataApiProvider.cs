using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Interfaces;
using System.Net.Http;

namespace Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider
{
    /// <summary>
    /// Contracts Data Api Provider.
    /// </summary>
    public abstract class ContractsDataApiProvider : BaseApiClient<ContractsDataApiConfiguration>, IHttpClientApiProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractsDataApiProvider"/> class.
        /// </summary>
        /// <param name="authenticationService">Authentication service.</param>
        /// <param name="httpClient">HttpClient.</param>
        /// <param name="configurationOptions">Configuration options.</param>
        public ContractsDataApiProvider(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<ContractsDataApiConfiguration> configurationOptions)
            : base(authenticationService, httpClient, configurationOptions)
        {
        }
    }
}
