using System.Collections.Generic;
using System.Linq;

namespace Pds.Contracts.Notifications.Services.Extensions
{
    /// <summary>
    /// IEnumerable extensions.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts a list to comma seperated string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>A comma seperated string.</returns>
        public static string ToCommaSeperatedString(this IEnumerable<string> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            return string.Join(",", list.ToArray());
        }


        /// <summary>
        /// Converts a list to semicolon seperated string.
        /// </summary>
        /// <param name="list">IEnumerable collection of strings.</param>
        /// <returns>A semicolon seperated string.</returns>
        public static string ToSemiColonSeperatedString(this IEnumerable<string> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            return string.Join("; ", list.ToArray());
        }

        /// <summary>
        /// Return true if collection has value.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="collection">IEnumerable collection of strings.</param>
        /// <returns>boolean value.</returns>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }
    }
}
