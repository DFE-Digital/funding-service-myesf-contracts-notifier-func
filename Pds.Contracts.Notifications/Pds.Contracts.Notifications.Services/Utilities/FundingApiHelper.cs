using Pds.Contracts.Notifications.Services.Configuration;

namespace Pds.Contracts.Notifications.Services.Utilities
{
    /// <summary>
    /// FundingApiHelper.
    /// </summary>
    public static class FundingApiHelper
    {
        /// <summary>
        /// FundingClaimGetById Endpoint.
        /// </summary>
        /// <param name="fundingClaimId">fundingClaimId.</param>
        /// <returns>url.</returns>
        public static string GetFundingClaimGetByIdUrl(int fundingClaimId)
        {
            return $"{Constants.FundingClaimByIdEndpoint}/{fundingClaimId}";
        }
    }
}
