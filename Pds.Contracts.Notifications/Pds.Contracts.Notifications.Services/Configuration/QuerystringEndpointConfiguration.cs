using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Querystring data for endpoint.
    /// </summary>
    public class QuerystringEndpointConfiguration
    {
        /// <summary>
        /// Gets or sets a collection of query parameters.
        /// </summary>
        public IDictionary<string, string> QueryParameters { get; set; }
    }
}
