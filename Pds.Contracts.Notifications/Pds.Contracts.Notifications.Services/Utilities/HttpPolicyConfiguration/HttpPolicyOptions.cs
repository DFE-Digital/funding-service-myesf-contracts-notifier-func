using System;

namespace Pds.Contracts.Notifications.Services.HttpPolicyConfiguration
{
    /// <summary>
    /// Http policy options.
    /// </summary>
    public class HttpPolicyOptions
    {
        /// <summary>
        /// Gets or sets the HTTP retry count.
        /// </summary>
        /// <value>
        /// The HTTP retry count.
        /// </value>
        public int HttpRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the HTTP retry backoff power.
        /// </summary>
        /// <value>
        /// The HTTP retry backoff power.
        /// </value>
        public double HttpRetryBackoffPower { get; set; } = 2;

        /// <summary>
        /// Gets or sets the circuit breaker tolerance count.
        /// </summary>
        /// <value>
        /// The circuit breaker tolerance count.
        /// </value>
        public int CircuitBreakerToleranceCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the circuit breaker duration of break.
        /// </summary>
        /// <value>
        /// The circuit breaker duration of break.
        /// </value>
        public TimeSpan CircuitBreakerDurationOfBreak { get; set; } = new TimeSpan(0, 0, 15);
    }
}