using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Interfaces;
using System.Net.Http;

namespace Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider
{
    /// <summary>
    /// Funding Claims Data Api Provider.
    /// </summary>
    public abstract class FundingClaimsDataApiProvider : BaseApiClient<FundingClaimsDataApiConfiguration>, IHttpClientApiProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimsDataApiProvider"/> class.
        /// </summary>
        /// <param name="authenticationService">Authentication service.</param>
        /// <param name="httpClient">HttpClient.</param>
        /// <param name="configurationOptions">Configuration options.</param>
        public FundingClaimsDataApiProvider(
            IAuthenticationService<FundingClaimsDataApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<FundingClaimsDataApiConfiguration> configurationOptions)
            : base(authenticationService, httpClient, configurationOptions)
        {
        }
    }
}
