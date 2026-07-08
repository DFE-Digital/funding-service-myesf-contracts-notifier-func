using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Contract Content To Be Signed Service.
    /// </summary>
    public class ContractContentToBeSignedService : ContractsDataApiProvider, IContractContentToBeSignedService
    {
        private readonly ILoggerAdapter<ContractContentToBeSignedService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractContentToBeSignedService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractContentToBeSignedService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            ILoggerAdapter<ContractContentToBeSignedService> logger,
            HttpClient httpClient)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(ContractContentToBeSignedMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractContentToBeSignedMessage)} with ContractId {message.ContractId}.");
            await Patch(ContractApiHelper.PrependSignedPageToDocumentUrl(), message.ContractId);
        }
    }
}
