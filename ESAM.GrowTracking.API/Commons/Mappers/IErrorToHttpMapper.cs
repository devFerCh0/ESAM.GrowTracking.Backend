using ESAM.GrowTracking.Application.Commons.Types;

namespace ESAM.GrowTracking.API.Commons.Mappers
{
    public interface IErrorToHttpMapper
    {
        int GetStatusCode(List<ErrorType> errorTypes);
    }
}