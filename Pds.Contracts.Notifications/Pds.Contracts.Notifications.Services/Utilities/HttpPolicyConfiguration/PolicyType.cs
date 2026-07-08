namespace Pds.Contracts.Notifications.Services.HttpPolicyConfiguration
{
    /// <summary>
    /// Http transient fault handing policy.
    /// </summary>
    /// <value>
    /// The type of the policy.
    /// </value>
    public enum PolicyType
    {
        /// <summary>
        /// Retry policy.
        /// </summary>
        Retry,

        /// <summary>
        /// Circuit breaker policy.
        /// </summary>
        CircuitBreaker,
    }
}