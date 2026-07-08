namespace Pds.Contracts.Notifications.Services.Models.ServiceBusMessages
{
    /// <summary>
    /// Defines a message detailing contract Query Message.
    /// </summary>
    public class ContractsQueryEmailMessage
    {
        /// <summary>
        /// Gets or sets the username of the provider sending the query.
        /// </summary>
        public string ProviderUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the provider sending the query.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the provider sending the query.
        /// </summary>
        public string ProviderEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the title of the contract being queried.
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Gets or sets the reason that the contract is being queried.
        /// </summary>
        public string QueryReason { get; set; }

        /// <summary>
        /// Gets or sets the details of why the contract is being queried.
        /// </summary>
        public string QueryDetail { get; set; }
    }
}
