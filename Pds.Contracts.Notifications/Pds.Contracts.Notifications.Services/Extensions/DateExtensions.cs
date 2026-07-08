using System;

namespace Pds.Contracts.Notifications.Services.Extensions
{
    /// <summary>
    /// Date Extensions.
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Extension method for displaying the datetime string.
        /// </summary>
        /// <param name="dateTime">DateTime.</param>
        /// <returns>DateTime string with custom display format.</returns>
        public static string DisplayFormat(this DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Local:
                    dateTime = dateTime.ToUniversalTime();
                    break;
                case DateTimeKind.Unspecified:
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    break;
                default:
                    break;
            }

            var dateString = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"))
                      .ToString("d MMMM yyyy a\\t h:mmtt");

            return dateString.Replace(dateString[^2..], dateString[^2..].ToLower());
        }
    }
}