using ESAM.GrowTracking.API.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Helpers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESAM.GrowTracking.API.Commons.Converters
{
    public sealed class JsonEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var t = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return t.IsEnum;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            var converterType = typeof(JsonEnumConverter<>).MakeGenericType(enumType);
            var converter = (JsonConverter?)Activator.CreateInstance(converterType, [typeToConvert]);
            return converter;
        }

        private sealed class JsonEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
        {
            private readonly bool _isNullable;
            private readonly Type _enumType;
            private readonly bool _isFlagEnum;

            public JsonEnumConverter(Type typeToConvert)
            {
                _isNullable = Nullable.GetUnderlyingType(typeToConvert) != null;
                _enumType = typeof(TEnum);
                _isFlagEnum = _enumType.GetCustomAttribute<FlagsAttribute>() != null;
            }

            public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    if (_isNullable)
                        return default;
                    throw new APIException($"Cannot convert null to {_enumType.Name}.");
                }
                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (reader.TryGetInt64(out var l))
                    {
                        var enumObj = Enum.ToObject(_enumType, l);
                        if (Enum.IsDefined(_enumType, enumObj) || _isFlagEnum)
                            return (TEnum)enumObj;
                        throw new APIException($"Value {l} is not defined for enum type {_enumType.Name}.");
                    }
                    if (reader.TryGetUInt64(out var ul))
                    {
                        var enumObj = Enum.ToObject(_enumType, ul);
                        if (Enum.IsDefined(_enumType, enumObj) || _isFlagEnum)
                            return (TEnum)enumObj;
                        throw new APIException($"Value {ul} is not defined for enum type {_enumType.Name}.");
                    }
                    throw new APIException($"Unable to parse number for enum {_enumType.Name}.");
                }
                if (reader.TokenType == JsonTokenType.String)
                {
                    var raw = reader.GetString() ?? string.Empty;
                    if (EnumHelper.TryParseFlexible<TEnum>(raw, out var parsed))
                        return parsed;
                    throw new APIException($"'{raw}' is not a valid value for enum {_enumType.Name}.");
                }
                throw new APIException($"Unexpected token parsing enum. Token: {reader.TokenType}");
            }

            public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            {
                var enumType = _enumType;
                var valueUInt64 = Convert.ToUInt64(value);
                if (_isFlagEnum)
                {
                    if (valueUInt64 == 0)
                    {
                        var zeroField = enumType.GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(f => Convert.ToUInt64(f.GetValue(null)!) == 0);
                        if (zeroField != null)
                        {
                            writer.WriteStringValue(EnumHelper.GetStringValue((TEnum)zeroField.GetValue(null)!));
                            return;
                        }
                        writer.WriteNumberValue(0);
                        return;
                    }
                    var parts = new List<string>();
                    foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        var fieldValue = (TEnum)field.GetValue(null)!;
                        var fieldUInt64 = Convert.ToUInt64(fieldValue);
                        if (fieldUInt64 != 0 && (valueUInt64 & fieldUInt64) == fieldUInt64)
                            parts.Add(EnumHelper.GetStringValue(fieldValue));
                    }
                    if (parts.Count > 0)
                    {
                        writer.WriteStringValue(string.Join(", ", parts));
                        return;
                    }
                    writer.WriteNumberValue(valueUInt64);
                    return;
                }
                var str = EnumHelper.GetStringValue(value);
                writer.WriteStringValue(str);
            }
        }
    }
}