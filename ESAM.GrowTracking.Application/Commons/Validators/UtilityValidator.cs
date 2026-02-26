using System.Text.RegularExpressions;
using ESAM.GrowTracking.Application.Commons.Helpers;

namespace ESAM.GrowTracking.Application.Commons.Validators
{
    public class UtilityValidator
    {
        public static bool IsValidCredential(string credential)
        {
            if (string.IsNullOrWhiteSpace(credential))
                return false;
            var cleanCredential = credential.Trim();
            if (cleanCredential.Contains('@'))
            {
                var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                var regex = new Regex(emailRegex);
                return regex.IsMatch(cleanCredential);
            }
            else
            {
                var usernameRegex = @"^[a-zA-Z0-9_.-]+$";
                var regex = new Regex(usernameRegex);
                return regex.IsMatch(cleanCredential);
            }
        }

        public static bool IsValidGuid(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return true;
            var clean = deviceId.Trim();
            if (Guid.TryParse(clean, out _))
                return true;
            var deviceIdRegex = @"^[a-zA-Z0-9\-_:.]{3,128}$";
            var regex = new Regex(deviceIdRegex);
            return regex.IsMatch(clean);
        }

        public static bool ContainsControlChars(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;
            return !value.Any(char.IsControl);
        }

        public static bool BeAValidEnum<TEnum>(string? value) where TEnum : struct, Enum
        {
            return EnumHelper.TryParseFlexible<TEnum>(value, out _);
        }
    }
}