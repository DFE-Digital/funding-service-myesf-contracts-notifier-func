using System;
using System.Text.Json.Serialization;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Contract object.
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// Gets or sets contract Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets contract Ukprn.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the funding.
        /// </summary>
        /// <value>
        /// The type of the funding.
        /// </value>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractFundingType FundingType { get; set; }

        /// <summary>
        /// Gets or sets the type of the amendment.
        /// </summary>
        /// <value>
        /// The type of the amendment.
        /// </value>
        public ContractAmendmentType AmendmentType { get; set; }

        /// <summary>
        /// Gets or sets the parent contract number.
        /// </summary>
        /// <value>
        /// The parent contract number.
        /// </value>
        public string ParentContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the display name of the signed by.
        /// </summary>
        /// <value>
        /// The display name of the signed by.
        /// </value>
        public string SignedByDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the signed by.
        /// </summary>
        /// <value>
        /// The signed by.
        /// </value>
        public string SignedBy { get; set; }

        /// <summary>
        /// Gets or sets the signed on.
        /// </summary>
        /// <value>
        /// The signed on.
        /// </value>
        public DateTime? SignedOn { get; set; }

        /// <summary>
        /// Gets or sets the content of the contract.
        /// </summary>
        /// <value>
        /// The content of the contract.
        /// </value>
        public virtual ContractContent ContractContent { get; set; }

        /// <summary>
        /// Gets a value indicating whether the contract id, version and number.
        /// </summary>
        public string ContractDisplayText => $"{this.Id}-{this.ContractNumber}-{this.ContractVersion}";

        /// <summary>
        /// Gets a value indicating whether the document is contract or agreement.
        /// </summary>
        /// <value>
        /// Determines whether the document is contract or agreement.
        /// </value>
        internal string DocumentType => this.FundingType != ContractFundingType.Levy ? "contract" : "agreement";
    }
}