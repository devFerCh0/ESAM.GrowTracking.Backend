using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Application.Commons.Helpers
{
    public static class EnumHelper
    {
        public static string GetStringValue<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var valueAsUInt64 = Convert.ToUInt64(enumValue);
            var isFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
            if (isFlags && valueAsUInt64 != 0)
            {
                var pieces = new List<string>();
                foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var fieldValue = (TEnum)field.GetValue(null)!;
                    var fieldUInt64 = Convert.ToUInt64(fieldValue);
                    if (fieldUInt64 != 0 && (valueAsUInt64 & fieldUInt64) == fieldUInt64)
                        pieces.Add(GetSingleMemberString(field, fieldValue));
                }
                if (pieces.Count > 0)
                    return string.Join(", ", pieces);
            }
            var member = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            if (member == null)
                return enumValue.ToString();
            return GetSingleMemberString(member, enumValue);
        }

        private static string GetSingleMemberString(MemberInfo member, object enumValue)
        {
            var display = member.GetCustomAttribute<DisplayAttribute>();
            if (display != null && !string.IsNullOrWhiteSpace(display.Name))
                return display.Name!;
            var desc = member.GetCustomAttribute<DescriptionAttribute>();
            if (desc != null && !string.IsNullOrWhiteSpace(desc.Description))
                return desc.Description!;
            var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMember != null && !string.IsNullOrWhiteSpace(enumMember.Value))
                return enumMember.Value!;
            return enumValue.ToString() ?? string.Empty;
        }

        public static bool TryParseFromString<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var enumType = typeof(TEnum);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var trimmed = value.Trim();
            var isFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
            if (isFlags && trimmed.Contains(','))
            {
                var tokens = trimmed.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
                ulong combined = 0;
                foreach (var t in tokens)
                {
                    if (!TryParseSingleToken(enumType, underlyingType, t, out var parsed))
                        return false;
                    combined |= Convert.ToUInt64(parsed);
                }
                result = (TEnum)Enum.ToObject(enumType, combined);
                return true;
            }
            if (TryParseSingleToken(enumType, underlyingType, trimmed, out var singleParsed))
            {
                result = (TEnum)singleParsed;
                return true;
            }
            return false;
        }

        public static bool TryParseFlexible<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum => TryParseFromString<TEnum>(value, out result);

        private static bool TryParseSingleToken(Type enumType, Type underlyingType, string token, out object result)
        {
            result = Activator.CreateInstance(enumType)!;
            if (TryParseNumericToken(underlyingType, token, out var numericParsed))
            {
                var candidate = Enum.ToObject(enumType, numericParsed);
                if (Enum.IsDefined(enumType, candidate))
                {
                    result = candidate;
                    return true;
                }
                if (enumType.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    result = candidate;
                    return true;
                }
            }
            var names = Enum.GetNames(enumType);
            var matchedName = names.FirstOrDefault(n => string.Equals(n, token, StringComparison.OrdinalIgnoreCase));
            if (matchedName != null)
                if (Enum.TryParse(enumType, matchedName, ignoreCase: true, out var byName))
                {
                    result = byName!;
                    return true;
                }
            foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var display = field.GetCustomAttribute<DisplayAttribute>();
                if (display != null && !string.IsNullOrWhiteSpace(display.Name) && string.Equals(display.Name, token, StringComparison.OrdinalIgnoreCase))
                {
                    result = field.GetValue(null)!;
                    return true;
                }
                var desc = field.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null && !string.IsNullOrWhiteSpace(desc.Description) && string.Equals(desc.Description, token, StringComparison.OrdinalIgnoreCase))
                {
                    result = field.GetValue(null)!;
                    return true;
                }
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                if (enumMember != null && !string.IsNullOrWhiteSpace(enumMember.Value) && string.Equals(enumMember.Value, token, StringComparison.OrdinalIgnoreCase))
                {
                    result = field.GetValue(null)!;
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseNumericToken(Type underlyingType, string token, out object numeric)
        {
            numeric = 0!;
            var tc = Type.GetTypeCode(underlyingType);
            switch (tc)
            {
                case TypeCode.SByte:
                    if (sbyte.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sbyteV))
                    {
                        numeric = sbyteV;
                        return true;
                    }
                    break;
                case TypeCode.Byte:
                    if (byte.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteV))
                    {
                        numeric = byteV;
                        return true;
                    }
                    break;
                case TypeCode.Int16:
                    if (short.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shortV))
                    {
                        numeric = shortV;
                        return true;
                    }
                    break;
                case TypeCode.UInt16:
                    if (ushort.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ushortV))
                    {
                        numeric = ushortV;
                        return true;
                    }
                    break;
                case TypeCode.Int32:
                    if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intV))
                    {
                        numeric = intV;
                        return true;
                    }
                    break;
                case TypeCode.UInt32:
                    if (uint.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uintV))
                    {
                        numeric = uintV;
                        return true;
                    }
                    break;
                case TypeCode.Int64:
                    if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longV))
                    {
                        numeric = longV;
                        return true;
                    }
                    break;
                case TypeCode.UInt64:
                    if (ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulongV))
                    {
                        numeric = ulongV;
                        return true;
                    }
                    break;
                default:
                    if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fallbackLong))
                    {
                        numeric = fallbackLong;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static TEnum ParseFromString<TEnum>(string value) where TEnum : struct, Enum
        {
            if (TryParseFromString<TEnum>(value, out var result))
                return result;
            throw new Exceptions.ApplicationException($"'{value}' no es un valor válido para {typeof(TEnum).Name}");
        }

        public static bool TryParseListFromString<TEnum>(IEnumerable<string>? values, out List<TEnum> result) where TEnum : struct, Enum
        {
            result = [];
            if (values == null)
                return false;
            foreach (var v in values)
            {
                if (!TryParseFromString<TEnum>(v, out var item))
                    return false;
                result.Add(item);
            }
            return true;
        }
    }
}