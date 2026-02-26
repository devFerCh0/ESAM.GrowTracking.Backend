using ESAM.GrowTracking.API.Commons.Exceptions;
using ESAM.GrowTracking.API.Commons.Mappers;
using ESAM.GrowTracking.Application.Commons.Result;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Commons.Extensions
{
    public static class ActionResultExtensions
    {
        public static ActionResult ToErrorActionResult(this Result result, IErrorToHttpMapper errorToHttpMapper)
        {
            Guard.AgainstNull(result, $"{nameof(result)} no puede ser nulo.");
            Guard.AgainstNull(errorToHttpMapper, $"{nameof(errorToHttpMapper)} no puede ser nulo.");
            var errorsPayload = result.Errors.Select(e => new { message = e.Message, fields = e.Fields }).ToList();
            var statusCode = errorToHttpMapper.GetStatusCode([.. result.Errors.Select(e => e.ErrorType)]);
            return new ObjectResult(new { success = false, errors = errorsPayload }) { StatusCode = statusCode };
        }
    }
}