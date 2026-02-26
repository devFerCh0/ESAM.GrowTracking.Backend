using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.ValueObjects;

namespace ESAM.GrowTracking.Application.Commons.Result
{
    public class Result : IResult
    {
        public bool IsSuccess { get; private set; }

        public bool IsFailure => !IsSuccess;

        public List<ErrorValueObject> Errors { get; private set; }

        protected Result(bool isSuccess, List<ErrorValueObject>? errors)
        {
            Guard.Against(isSuccess && errors is not null && errors.Count != 0, "Un resultado exitoso no puede contener errores.");
            Guard.Against(!isSuccess && (errors is null || errors.Count == 0), "Un resultado fallido debe contener al menos un error.");
            IsSuccess = isSuccess;
            Errors = [.. errors ?? []];
        }

        public static Result Ok() => new(true, null);

        public static Result Fail(ErrorValueObject error)
        {
            Guard.AgainstNull(error, $"{nameof(error)} no puede ser nulo");
            return new Result(false, [error]);
        }

        public static Result Fail(List<ErrorValueObject> errors)
        {
            Guard.AgainstNull(errors, $"{nameof(errors)} no puede ser nulo.");
            return new Result(false, errors);
        }

        public static Result Combine(params Result[] results)
        {
            Guard.AgainstNull(results, $"{nameof(results)} no puede ser nulo.");
            var failedErrors = results.Where(r => r != null && r.IsFailure).SelectMany(r => r.Errors).ToList();
            return failedErrors.Count != 0 ? Fail(failedErrors) : Ok();
        }
    }

    public sealed class Result<T> : Result, IResult<T>
    {
        public T Value { get; }

        private Result(bool isSuccess, T? value, List<ErrorValueObject>? errors) : base(isSuccess, errors)
        {
            if (isSuccess)
            {
                Guard.AgainstNull(value, $"{nameof(value)} no puede ser nulo.");
                Value = value!;
            }
            else
                Value = default!;
        }

        public static Result<T> Ok(T value) => new(true, value, null);

        public new static Result<T> Fail(ErrorValueObject error)
        {
            Guard.AgainstNull(error, $"{nameof(error)} no puede ser nulo.");
            return new Result<T>(false, default, [error]);
        }

        public new static Result<T> Fail(List<ErrorValueObject> errors)
        {
            Guard.AgainstNull(errors, $"{nameof(errors)} no puede ser nulo.");
            return new Result<T>(false, default, errors);
        }

        public Result AsResult() => IsSuccess ? Ok() : Fail(Errors);
    }
}