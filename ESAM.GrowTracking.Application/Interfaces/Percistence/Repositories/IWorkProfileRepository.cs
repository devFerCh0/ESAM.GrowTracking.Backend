using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories
{
    public interface IWorkProfileRepository : IRepository<WorkProfile, int>
    {
        Task<bool> IsValidWorkProfileTypeAsync(int id, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default);


        Task<WorkProfileType> GetWorkProfileTypeById(int id, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}