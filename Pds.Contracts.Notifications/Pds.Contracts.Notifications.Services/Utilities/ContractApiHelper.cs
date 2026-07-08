using Pds.Contracts.Notifications.Services.Configuration;

namespace Pds.Contracts.Notifications.Services.Utilities
{
    /// <summary>
    /// ContractApiHelper.
    /// </summary>
    public static class ContractApiHelper
    {
        /// <summary>
        /// Construct url to make a call to Contract data api.
        /// </summary>
        /// <param name="contractId">ContractId.</param>
        /// <returns>url.</returns>
        public static string GetContractByIdUrl(int contractId)
        {
            return $"{Constants.ContractGetByIdEndpoint}/{contractId}";
        }

        /// <summary>
        /// Construct url to make a call to Contract data api for prepending the signed page to document.
        /// </summary>
        /// <param name="contractId">ContractId.</param>
        /// <returns>url.</returns>
        public static string PrependSignedPageToDocumentUrl()
        {
            return $"{Constants.PrependSignedPageToDocumentEndpoint}";
        }

        /// <summary>
        /// Create contracts data api query string with contract number, version and kprn.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="version">Version.</param>
        /// <param name="ukprn">Ukprn.</param>
        /// <returns>API query string.</returns>
        public static string CreateContractQueryString(string contractNumber, int version, int ukprn)
        {
            return $"{Constants.ContractDetailsEndpoint}?contractNumber={contractNumber}&versionNumber={version}&ukprn={ukprn}";
        }
    }
}
