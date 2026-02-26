using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using FluentValidation.Results;

namespace ESAM.GrowTracking.Application.Commons.Extensions
{
    public static class FluentValidationExtensions
    {
        public static List<ErrorValueObject> ToDomainErrors(this ValidationResult validationResult)
        {
            Guard.AgainstNull(validationResult, $"{nameof(validationResult)} no puede ser nulo.");
            return [.. validationResult.Errors
                .GroupBy(vf => vf.PropertyName ?? string.Empty)
                .Select(group =>
                {
                    var message = string.Join("; ", group.Select(f => f.ErrorMessage));
                    var propertyName = group.Key;
                    if (string.IsNullOrWhiteSpace(propertyName))
                        return ErrorValueObject.Validation(message);
                    return ErrorValueObject.Validation(message, propertyName);
                })];
        }
    }
}