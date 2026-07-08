using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pds.Contracts.Notifications.Services.Utilities
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Utility method to replace tokens from format string.
        /// </summary>
        /// <typeparam name="T">Type that is passed in for token replacement.</typeparam>
        /// <param name="format">Format string with tokens.</param>
        /// <param name="tokenValues">Instance of <typeparamref name="T"/> that contains token values.</param>
        /// <returns>Replaced string.</returns>
        public static string ReplaceTokens<T>(this string format, T tokenValues)
            where T : class
        {
            var tokenRegex = new Regex(@"\{([^}]+)\}");
            if (!tokenRegex.IsMatch(format))
            {
                return format;
            }

            var matches = tokenRegex.Matches(format);
            switch (tokenValues)
            {
                case Dictionary<string, string> tokenDictionary:
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count > 1 && tokenDictionary.TryGetValue(match.Groups[1].Value, out var replacementValue))
                            {
                                format = format.Replace(match.Value, replacementValue);
                            }
                        }

                        return format;
                    }

                default:
                    var properties = typeof(T).GetProperties();

                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count > 1 && properties.Any(p => p.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase)))
                        {
                            var replacementValue = properties.Single(p => p.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase)).GetValue(tokenValues)?.ToString();
                            format = format.Replace(match.Value, replacementValue ?? match.Value);
                        }
                    }

                    return format;
            }
        }
    }
}