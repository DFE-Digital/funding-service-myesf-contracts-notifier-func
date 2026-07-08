using System.Runtime.Serialization;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Defines a message for a reminder contract.
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Sfa.Sfs.Contracts.Messaging")]
    public class ContractReminderMessage
    {
        /// <summary>
        /// Message Type value to be attached to the properties of the message when sent to the Monolith.
        /// </summary>
        public const string MessageProcessor_ContractReminderMessage = "Sfa.Sfs.Contracts.Messaging.ContractReminderMessage";

        /// <summary>
        /// Gets or sets the id of the contract that still needs to be signed (email reminder).
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = true, Order = 10)]
        public int ContractId { get; set; }
    }
}
