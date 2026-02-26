using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Types;

namespace ESAM.GrowTracking.Application.Commons.ValueObjects
{
    public sealed class ErrorValueObject : IEquatable<ErrorValueObject>
    {
        public string Message { get; }

        public ErrorType ErrorType { get; }

        public List<string> Fields { get; }

        private ErrorValueObject(string message, ErrorType errorType, List<string>? fields = null)
        {
            Guard.AgainstNullOrWhiteSpace(message, $"El parámeto '{nameof(message)}' no puede ser nulo ni vacio.");
            Message = message;
            ErrorType = errorType;
            Fields = fields?.ToList() ?? [];
        }

        public static ErrorValueObject Validation(string message, params string[] fields) => new(message, ErrorType.Validation, fields?.ToList());

        public static ErrorValueObject NotFound(string message) => new(message, ErrorType.NotFound);

        public static ErrorValueObject Unauthorized(string message) => new(message, ErrorType.Unauthorized);

        public static ErrorValueObject Forbidden(string message) => new(message, ErrorType.Forbidden);

        public static ErrorValueObject Conflict(string message) => new(message, ErrorType.Conflict);

        public static ErrorValueObject BusinessRule(string message) => new(message, ErrorType.BusinessRule);

        public static ErrorValueObject UnprocessableEntity(string message) => new(message, ErrorType.UnprocessableEntity);

        public static ErrorValueObject Locked(string message) => new(message, ErrorType.Locked);

        public static ErrorValueObject ServerError(string message) => new(message, ErrorType.ServerError);

        public override bool Equals(object? obj) => Equals(obj as ErrorValueObject);

        public bool Equals(ErrorValueObject? error)
        {
            if (error is null)
                return false;
            if (ReferenceEquals(this, error))
                return true;
            return Message == error.Message && ErrorType == error.ErrorType && Fields.SequenceEqual(error.Fields);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Message.GetHashCode();
                hash = hash * 23 + ErrorType.GetHashCode();
                foreach (var field in Fields)
                    hash = hash * 23 + field.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            var fieldsPart = Fields != null && Fields.Count > 0 ? $" Fields=[{string.Join(",", Fields)}]" : string.Empty;
            return $"Error(Message={Message}, Type={ErrorType}{fieldsPart})";
        }
    }
}