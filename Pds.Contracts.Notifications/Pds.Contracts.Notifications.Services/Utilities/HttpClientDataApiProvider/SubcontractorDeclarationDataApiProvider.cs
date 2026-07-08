using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Interfaces;
using System.Net.Http;

namespace Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider
{
    /// <summary>
    /// Subcontractor Declaration Data Api Provider.
    /// </summary>
    public class SubcontractorDeclarationDataApiProvider : BaseApiClient<SubcontractorDeclarationDataApiConfiguration>, IHttpClientApiProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationDataApiProvider"/> class.
        /// </summary>
        /// <param name="authenticationService">Authentication service.</param>
        /// <param name="httpClient">HttpClient.</param>
        /// <param name="configurationOptions">Configuration options.</param>
        public SubcontractorDeclarationDataApiProvider(
            IAuthenticationService<SubcontractorDeclarationDataApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<SubcontractorDeclarationDataApiConfiguration> configurationOptions)
            : base(authenticationService, httpClient, configurationOptions)
        {
        }
    }
}
