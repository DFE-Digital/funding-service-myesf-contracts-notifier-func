using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Registrations;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.HttpPolicyConfiguration;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Implementations.Contracts;
using Pds.Contracts.Notifications.Services.Implementations.FeedReed;
using Pds.Contracts.Notifications.Services.Implementations.FundingClaims;
using Pds.Contracts.Notifications.Services.Implementations.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Interfaces.FeedReed;
using Pds.Contracts.Notifications.Services.Interfaces.FundingClaims;
using Pds.Contracts.Notifications.Services.Interfaces.SubcontractorDeclarations;
using Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.ApiClient.Services;
using Pds.Core.Azure;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.DfESignIn.Services;
using Pds.Core.Notification.Registration;
using Pds.Core.Utils.Implementations;
using Pds.Core.Utils.Interfaces;
using Polly;

namespace Pds.Contracts.Notifications.Services.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class FeatureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for the current feature to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the feature's services to.</param>
        /// <param name="config">The <see cref="IConfiguration"/> elements for the current service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddFeatureServices(this IServiceCollection services, IConfiguration config)
        {
            var policyRegistry = services.AddPolicyRegistry();
            var policies = new PolicyType[] { PolicyType.Retry, PolicyType.CircuitBreaker };

            // Configure Polly Policies for IContractsApproverService HttpClient
            services
                .AddPolicies<IContractNotificationService>(config, policyRegistry)
                .AddHttpClient<IContractNotificationService, ContractNotificationService, ContractsDataApiConfiguration>(config, policies);

            services
                .AddPolicies<IHttpClientApiProvider>(config, policyRegistry);

            services
                .AddHttpClient<IHttpClientApiProvider, FundingClaimsDataApiProvider, FundingClaimsDataApiConfiguration>(config, policies);

            services
                .AddHttpClient<IHttpClientApiProvider, ContractsDataApiProvider, ContractsDataApiConfiguration>(config, policies);

            services
                .AddHttpClient<IHttpClientApiProvider, SubcontractorDeclarationDataApiProvider, SubcontractorDeclarationDataApiConfiguration>(config, policies);

            services.AddAzureMessagingServiceBusAllQueues(config);

            services.AddNotificationClient(config);

            // Configure service for audit
            services.AddAuditApiClient(config, policyRegistry);

            services.Configure<PublicApiSettings>(config.GetSection("DfESignin:PublicApi"));
            services.AddScoped(resolver =>
            {
                return resolver.GetService<IOptions<PublicApiSettings>>().Value;
            });

            services
                .AddHttpClient<IDfESignInPublicApi, DfESignInPublicApi>()
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(3));

            // Need to allow resue of Azure AAD auth tokens
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient(typeof(IAuthenticationService<>), typeof(AuthenticationService<>));

            services.AddScoped(typeof(IContractReminderProcessingService), typeof(ContractReminderProcessingService));

            services.AddScoped<IContractStatusChangePublisher, ContractStatusChangePublisher>();

            services.AddScoped<IFeedReadExceptionService, FeedReadExceptionService>();
            services.AddScoped<IFeedReadThresholdExceededWarningService, FeedReadThresholdExceededWarningService>();
            services.AddScoped<IProcessContractFromFeedExceptionService, ProcessContractFromFeedExceptionService>();
            services.AddScoped<IContractReadyToReviewEmailService, ContractReadyToReviewEmailService>();
            services.AddScoped<IContractWithdrawnEmailService, ContractWithdrawnEmailService>();
            services.AddScoped<IContractReminderEmailService, ContractReminderEmailService>();
            services.AddScoped<IContractApprovedEmailService, ContractApprovedEmailService>();
            services.AddScoped<IContractReadyToSignEmailService, ContractReadyToSignEmailService>();
            services.AddScoped<IContractsQueryEmailService, ContractsQueryEmailService>();
            services.AddScoped<IContractContentToBeSignedService, ContractContentToBeSignedService>();

            services.AddScoped<IFundingClaimSignedEmailService, FundingClaimSignedEmailService>();
            services.AddScoped<IFundingClaimReadyToSignEmailService, FundingClaimReadyToSignEmailService>();
            services.AddScoped<IFundingClaimReadyToViewEmailService, FundingClaimReadyToViewEmailService>();
            services.AddScoped<IFundingClaimWithdrawnEmailService, FundingClaimWithdrawnEmailService>();
            services.AddScoped<IReconciliationReadyToBeViewedEmailService, ReconciliationReadyToBeViewedEmailService>();

            services.AddScoped<ISubcontractorDeclarationEmailService, SubcontractorDeclarationEmailService>();

            return services;
        }
    }
}