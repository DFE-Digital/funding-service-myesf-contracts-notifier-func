using Pds.Contracts.Notifications.Services.Models;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// All constant literals are kept in this class.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Component that is sending the notification, equivalent to message handler type.
        /// </summary>
        public const string ComponentName = "pds.Contracts.Notifications";

        #region Service Bus Constants

        /// <summary>
        /// Component that is sending the notification, equivalent to message handler type.
        /// </summary>
        public const string Component = "Component";

        /// <summary>
        /// Audit user.
        /// </summary>
        public const string Audit_User_System = "System_NotificationFunction";

        /// <summary>
        /// Contract ReadyToReview Email Queue.
        /// </summary>
        public const string ContractReadyToReviewEmailQueue = "contractreadytoreviewemail";

        /// <summary>
        /// Contract Withdrawn Email Queue.
        /// </summary>
        public const string ContractWithdrawnEmailQueue = "contractwithdrawnemail";

        /// <summary>
        /// Contract Reminder Email Queue.
        /// </summary>
        public const string ContractReminderEmailQueue = "contractreminderemail";

        /// <summary>
        /// Contract Approved Email Queue.
        /// </summary>
        public const string ContractApprovedEmailQueue = "contractapprovedemail";

        /// <summary>
        /// Contract Ready To Sign Email Queue.
        /// </summary>
        public const string ContractReadyToSignEmailQueue = "contractreadytosignemail";

        /// <summary>
        /// Contracts Query Email Queue.
        /// </summary>
        public const string ContractsQueryEmailQueue = "contractsqueryemail";

        /// <summary>
        /// Contract Content To Be Signed Queue.
        /// </summary>
        public const string ContractContentToBeSignedQueue = "contractcontenttobesigned";

        /// <summary>
        /// Feed Read Threshold Exceeded Warning Email Queue.
        /// </summary>
        public const string FeedReadThresholdExceededWarningEmailQueue = "feedReadthresholdexceededwarning";

        /// <summary>
        /// Feed Read Exception Email Queue.
        /// </summary>
        public const string FeedReadExceptionEmailQueue = "feedreadexception";

        /// <summary>
        /// Funding Claim Signed Email Queue.
        /// </summary>
        public const string FundingClaimSignedEmailQueue = "fundingclaimsignedemail";

        /// <summary>
        /// Funding Claim Ready To Sign Email Queue.
        /// </summary>
        public const string FundingClaimReadyToSignEmailQueue = "fundingclaimreadytosignemail";

        /// <summary>
        /// Funding Claim Ready To View Email Queue.
        /// </summary>
        public const string FundingClaimReadyToViewEmailQueue = "fundingclaimreadytoviewemail";

        /// <summary>
        /// Funding Claim Withdrawn Email Queue.
        /// </summary>
        public const string FundingClaimWithdrawnEmailQueue = "fundingclaimwithdrawnemail";

        /// <summary>
        /// Reconciliation Ready To Be Viewed Email Queue.
        /// </summary>
        public const string ReconciliationReadyToBeViewedEmailQueue = "reconciliationreadytobeviewedemail";

        /// <summary>
        /// Subcontractor Declaration Email Queue.
        /// </summary>
        public const string SubcontractorDeclarationEmailQueue = "subcontractordeclarationemail";

        /// <summary>
        /// Process Contract From Feed Exception Email Queue.
        /// </summary>
        public const string ProcessContractFromFeedExceptionQueue = "processcontractfromfeedexception";

        /// <summary>
        /// Notifier ServiceBus Connection String Name.
        /// </summary>
        public const string NotifierServiceBusConnectionString = "NotifierServiceBusConnectionString";

        #endregion Service Bus Constants

        #region Requesting Service Types

        // RequestingService in NotifyServiceTemplateDetails from Azure storage.

        /// <summary>
        /// Requesting Service For Funding Claims Notification Service.
        /// </summary>
        public const string RequestingService_FundingClaims = "FundingClaims";

        /// <summary>
        /// Requesting Service For Contracts Notification Service.
        /// </summary>
        public const string RequestingService_Contracts = "Contracts";

        /// <summary>
        /// Requesting Service For Subcontractor Declarations Notification Service.
        /// </summary>
        public const string RequestingService_SubcontractorDeclarations = "SubcontractorDeclarations";

        #endregion Requesting Service Types

        #region Notification Message Types

        // PartitionKey in NotifyServiceTemplateDetails from Azure storage.

        /// <summary>
        /// Feed Read Exception Bookmark Not Matched Email Message Type.
        /// </summary>
        public const string MessageType_FeedReadExceptionBookmarkNotMatchedEmail = "FeedReadExceptionBookmarkNotMatched";

        /// <summary>
        /// Feed Read Exception Empty Page Email Message Type.
        /// </summary>
        public const string MessageType_FeedReadExceptionEmptyPageEmail = "FeedReadExceptionEmptyPage";

        /// <summary>
        /// Feed Read Threshold Exceeded Warning Email Message Type.
        /// </summary>
        public const string MessageType_FeedReadThresholdExceededWarningEmail = "FeedReadThresholdExceededWarning";

        /// <summary>
        /// Contract Ready To Review Email Message Type.
        /// </summary>
        public const string MessageType_ContractReadyToReviewEmail = "ContractReadyToReview";

        /// <summary>
        /// Contract Withdrawn By ESFA Email Message Type.
        /// </summary>
        public const string MessageType_ContractWithdrawnByESFAEmail = "ContractWithdrawnESFA";

        /// <summary>
        /// Contract Withdrawn By Provider Email Message Type.
        /// </summary>
        public const string MessageType_ContractWithdrawnByProviderEmail = "ContractWithdrawnProvider";

        /// <summary>
        /// Contract Reminder Email Message Type.
        /// </summary>
        public const string MessageType_ContractReminderEmail = "ContractReminder";

        /// <summary>
        /// Contract Approved Email Message Type.
        /// </summary>
        public const string MessageType_ContractApprovedEmail = "ContractSigned";

        /// <summary>
        /// Contract Ready To Sign For Signing Users Email Message Type.
        /// </summary>
        public const string MessageType_ContractReadyToSignEmail = "ContractReadyToSign";

        /// <summary>
        /// Contract Ready To Sign For View Only Users Email Message Type.
        /// </summary>
        public const string MessageType_ContractReadyToSignViewOnlyEmail = "ContractReadyToSignViewOnly";

        /// <summary>
        /// Contracts Query Email Message Type.
        /// </summary>
        public const string MessageType_ContractsQueryEmail = "ContractsQuery";

        /// <summary>
        /// Process Contract From Feed Exception Message Type.
        /// </summary>
        public const string MessageType_ProcessContractFromFeedException = "ProcessContractFromFeedException";

        /// <summary>
        /// Funding Claim Signed Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimSignedEmail = "FundingClaimSigned";

        /// <summary>
        /// Funding Claim Ready To Sign For Signing Users Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimReadyToSignEmail = "FundingClaimReadyToSign";

        /// <summary>
        /// Funding Claim Ready To Sign For View Only Users Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimReadyToSignViewOnlyEmail = "FundingClaimReadyToSignViewOnly";

        /// <summary>
        /// Funding Claim Ready To View Submitted Date Available Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimReadyToViewDateAvailableEmail = "FundingClaimReadyToViewDateAvailable";

        /// <summary>
        /// Funding Claim Withdrawn Not Signed Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimWithdrawnNotSignedEmail = "FundingClaimWithdrawnNotSigned";

        /// <summary>
        /// Funding Claim Withdrawn When Previous Version Signed Email Message Type.
        /// </summary>
        public const string MessageType_FundingClaimWithdrawnPreviousVersionSignedEmail = "FundingClaimWithdrawnPreviousVersionSigned";

        /// <summary>
        /// Reconciliation Ready To Be Viewed Email Message Type.
        /// </summary>
        public const string MessageType_ReconciliationReadyToBeViewedEmail = "ReconciliationReadyToView";

        /// <summary>
        /// Subcontractor Return Submission Full Return Email Message Type.
        /// </summary>
        public const string MessageType_SubcontractorReturnSubmissionFullReturnEmail = "SubcontractorReturnSubmissionFullReturn";

        /// <summary>
        /// Subcontractor Return Submission Nil Return Email Message Type.
        /// </summary>
        public const string MessageType_SubcontractorReturnSubmissionNilReturnEmail = "SubcontractorReturnSubmissionNilReturn";

        #endregion Notification Message Types

        #region API Endpoints

        #region Contract API Endpoints

        /// <summary>
        /// API Endpoint for ContractReminder.
        /// </summary>
        public static readonly string ContractReminderEndpoint = "/api/contractReminders";

        /// <summary>
        /// API Endpoint for ContractReminderPatch.
        /// </summary>
        public static readonly string ContractReminderPatchEndpoint = "/api/contractReminder";

        /// <summary>
        /// API Endpoint for ContractDetails.
        /// </summary>
        public static readonly string ContractDetailsEndpoint = "/api/contractByNumberVersionUkprn";

        /// <summary>
        /// API Endpoint for ContractGetById.
        /// </summary>
        public static readonly string ContractGetByIdEndpoint = "/api/contract";

        /// <summary>
        /// API Endpoint for ContractGetById.
        /// </summary>
        public static readonly string PrependSignedPageToDocumentEndpoint = "/api/contract/prependSignedPage";

        #endregion Contract API Endpoints

        #region FundingClaim API Endpoints

        /// <summary>
        /// API Endpoint for FundingClaimGetById.
        /// </summary>
        public static readonly string FundingClaimByIdEndpoint = "/api/FundingClaim/GetFundingClaimById";

        /// <summary>
        /// API Endpoint for PreviouslySignedVersionOfFundingClaimById.
        /// </summary>
        public static readonly string PreviouslySignedVersionOfFundingClaimByIdEndpoint = "/api/FundingClaim/GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId";

        /// <summary>
        /// API Endpoint for ReconciliationGetById.
        /// </summary>
        public static readonly string ReconciliationGetByIdEndpoint = "/api/Reconciliation/GetReconciliationById";

        #endregion FundingClaim API Endpoints

        #region SubcontractorDeclaration API Endpoints

        /// <summary>
        /// API Endpoint for FullSubcontractorDeclarationGetById.
        /// </summary>
        public static readonly string FullSubcontractorDeclarationGetByIdEndpoint = "/api/SubcontractorDeclaration/GetFullSubcontractorDeclarationById";

        #endregion SubcontractorDeclaration API Endpoints

        #endregion API Endpoints

        /// <summary>
        /// Message that should be used for auditing contract status change notification when forwarded to legacy service bus queue.
        /// </summary>
        public static readonly string ContractNotificationForwardedMessage = $"A contract notification has been forwarded to the [MessageProcessor] by [{{{Component}}}] for contract [{{{nameof(Contract.ContractNumber)}}}] Version [{{{nameof(Contract.ContractVersion)}}}] with the status of [{{{nameof(Contract.Status)}}}]";

        /// <summary>
        /// Message that should be used for auditing contract status change notification when forwarded to legacy service bus queue.
        /// </summary>
        public static readonly string AuditMessage = "{0} processed and published to SharedEmailprocessorQueue.";

        /// <summary>
        /// Message that should be used for application insight.
        /// </summary>
        public static readonly string LogMessage = "Contract [{0}], {1}.";
    }
}