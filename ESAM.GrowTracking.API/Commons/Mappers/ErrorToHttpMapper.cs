using ESAM.GrowTracking.API.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Types;

namespace ESAM.GrowTracking.API.Commons.Mappers
{
    public class ErrorToHttpMapper : IErrorToHttpMapper
    {
        private static readonly Dictionary<ErrorType, int> _map = new()
        {
            { ErrorType.Validation, StatusCodes.Status400BadRequest },
            { ErrorType.BusinessRule, StatusCodes.Status400BadRequest },
            { ErrorType.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity },
            { ErrorType.NotFound, StatusCodes.Status404NotFound },
            { ErrorType.Unauthorized, StatusCodes.Status401Unauthorized },
            { ErrorType.Forbidden, StatusCodes.Status403Forbidden },
            { ErrorType.Conflict, StatusCodes.Status409Conflict },
            { ErrorType.Locked, StatusCodes.Status423Locked },
            { ErrorType.ServerError, StatusCodes.Status500InternalServerError }
        };

        public int GetStatusCode(List<ErrorType> errorTypes)
        {
            Guard.AgainstNull(errorTypes, $"{nameof(errorTypes)} no puede ser nulo.");
            Guard.Against(errorTypes.Count == 0, "Debe existir al menos un tipo de error.");
            var codes = errorTypes.Select(type => _map.TryGetValue(type, out var status) ? status : StatusCodes.Status400BadRequest);
            return codes.Max();
        }
    }
}