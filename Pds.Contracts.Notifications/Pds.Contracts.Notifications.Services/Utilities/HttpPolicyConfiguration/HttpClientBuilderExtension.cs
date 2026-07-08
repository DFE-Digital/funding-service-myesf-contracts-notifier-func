using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Core.ApiClient;
using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.HttpPolicyConfiguration
{
    /// <summary>
    /// Extension methods to allow dependcy injection of Polly in to Http Client.
    /// </summary>
    public static class HttpClientBuilderExtension
    {
        /// <summary>
        /// Adds polly policies to the Http Client for the given service.
        /// </summary>
        /// <typeparam name="TService">The service to apply Polly Http Client settings to.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <typeparam name="TClientOptions">The configuration options for the service.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> elements for the current service.</param>
        /// <param name="policies">A collection <see cref="PolicyType"/> to apply to the Http Client.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IHttpClientBuilder AddHttpClient<TService, TImplementation, TClientOptions>(
                   this IServiceCollection services,
                   IConfiguration configuration,
                   IList<PolicyType> policies)
                   where TService : class
                   where TImplementation : class, TService
                   where TClientOptions : BaseApiClientConfiguration, new()
        {
            var httpClientBuilder = services
                                    .Configure<TClientOptions>(c => configuration?.Bind(typeof(TClientOptions).Name, c))
                                    .AddHttpClient<TService, TImplementation>();

            foreach (var policy in policies)
            {
                httpClientBuilder.AddPolicyHandlerFromRegistry($"{typeof(TService).Name}_{policy}");
            }

            return httpClientBuilder;
        }
    }
}