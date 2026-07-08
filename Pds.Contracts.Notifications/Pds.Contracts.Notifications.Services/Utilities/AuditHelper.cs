using Pds.Audit.Api.Client.Enumerations;
using Pds.Contracts.Notifications.Services.Configuration;

namespace Pds.Contracts.Notifications.Services.Utilities
{
    /// <summary>
    /// Helper class for Audit Logs.
    /// </summary>
    public static class AuditHelper
    {
        /// <summary>
        /// Helper for creating audit object.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="ukprn">Ukprn.</param>
        /// <param name="action">Action.</param>
        /// <param name="user">User.</param>
        /// <param name="severity">Severity.</param>
        /// <returns>Contract Message.</returns>
        public static Audit.Api.Client.Models.Audit ConstructAuditObject(
            string message,
            int ukprn,
            ActionType action,
            string user = Constants.Audit_User_System,
            SeverityLevel severity = SeverityLevel.Information)
        {
            return new Audit.Api.Client.Models.Audit
            {
                Severity = severity,
                Action = action,
                Ukprn = ukprn,
                Message = message,
                User = user
            };
        }

        /// <summary>
        /// Helper for creating audit object when no users have expected roles.
        /// </summary>
        /// <param name="message">Additional Information.</param>
        /// <param name="ukprn">Ukprn.</param>
        /// <returns>Contract Message.</returns>
        public static Audit.Api.Client.Models.Audit AuditUsersWithNoExpectedRoles(
            string message,
            int? ukprn = null)
        {
            return new Audit.Api.Client.Models.Audit
            {
                Severity = SeverityLevel.Error,
                Action = ActionType.EmailNotSentAsNoRole,
                Ukprn = ukprn,
                Message = message,
                User = Constants.Audit_User_System
            };
        }

        /// <summary>
        /// Update audit object with error message and serverity.
        /// </summary>
        /// <param name="auditData">auditData.</param>
        /// <param name="message">message.</param>
        public static void UpdateErrorMessage(this Audit.Api.Client.Models.Audit auditData, string message)
        {
            auditData.Message = message;
            auditData.Severity = SeverityLevel.Error;
        }
    }
}
