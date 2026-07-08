using System;

namespace Pds.Contracts.Notifications.Services.Extensions
{
    /// <summary>
    /// Enum Attribute Extensions.
    /// </summary>
    public static class EnumAttributeExtensions
    {
        /// <summary>
        /// Get Enum Attribute Property Value.
        /// </summary>
        /// <typeparam name="TEnum">Enum Type.</typeparam>
        /// <typeparam name="TAttribute">Attribute Typw.</typeparam>
        /// <param name="value">Enum Value.</param>
        /// <param name="property">Property.</param>
        /// <returns>Attibute Property Value.</returns>
        public static string GetPropertyValue<TEnum, TAttribute>(this TEnum value, Func<TAttribute, string> property)
            where TEnum : struct
            where TAttribute : Attribute
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (TAttribute)Attribute.GetCustomAttribute(field, typeof(TAttribute));
            return property(attribute);
        }
    }
}
