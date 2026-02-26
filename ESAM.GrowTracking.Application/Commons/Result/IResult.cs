using ESAM.GrowTracking.Application.Commons.ValueObjects;

namespace ESAM.GrowTracking.Application.Commons.Result
{
    public interface IResult
    {
        bool IsSuccess { get; }
        List<ErrorValueObject> Errors { get; }
    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
    }
}