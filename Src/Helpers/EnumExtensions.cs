using System.Reflection;
using System.Runtime.Serialization;

namespace Tsundoku.Helpers;

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
        where TEnum : struct, Enum
    {
        foreach (TEnum enumValue in Enum.GetValues<TEnum>())
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
        foreach (TEnum enumValue in Enum.GetValues<TEnum>())
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