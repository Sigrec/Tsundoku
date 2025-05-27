using System.Reflection;
using System.Runtime.Serialization;

namespace Tsundoku.Helpers
{
    public static class EnumExtensions
    {
        public static string GetEnumMemberValue(this Enum value)
        {
            return value.GetType()
                .GetField(value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>()
                ?.Value ?? value.ToString();
        }

        public static TEnum GetEnumValueFromMemberValue<TEnum>(this string stringValue, bool ignoreCase = true)
            where TEnum : struct, Enum // Constraint to ensure TEnum is an enum type
        {
            // Ensure TEnum is actually an enum
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"Type '{typeof(TEnum).Name}' is not an enum.");
            }

            // Get all enum members of the specified type
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                string memberValue = enumValue.GetEnumMemberValue(); // Use our existing extension method

                if (string.Equals(memberValue, stringValue, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return enumValue; // Found a match!
                }
            }

            // If no match found after checking all members' EnumMemberAttribute values and ToString() fallbacks
            throw new ArgumentException($"String '{stringValue}' does not correspond to any member of enum '{typeof(TEnum).Name}' via EnumMemberAttribute or ToString().");
        }

        public static TEnum GetEnumValueFromMemberValue<TEnum>(this string stringValue, TEnum defaultValue, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"Type '{typeof(TEnum).Name}' is not an enum.");
            }

            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                string memberValue = enumValue.GetEnumMemberValue();
                if (string.Equals(memberValue, stringValue, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return enumValue;
                }
            }
            return defaultValue;
        }

        public static bool TryGetEnumValueFromMemberValue<TEnum>(this string stringValue, out TEnum result, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                result = default;
                return false;
            }

            foreach (TEnum enumValue in Enum.GetValues<TEnum>())
            {
                string memberValue = enumValue.GetEnumMemberValue();
                if (string.Equals(memberValue, stringValue, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    result = enumValue;
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}