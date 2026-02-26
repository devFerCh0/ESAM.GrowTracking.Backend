using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class WorkProfile : IEntity<int>
    {
        private WorkProfile() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public WorkProfileType WorkProfileType { get; private set; }

        public bool IsDeleted { get; private set; }

        public List<UserWorkProfile> UserWorkProfiles { get; private set; } = [];

        public List<WorkProfilePermission> WorkProfilePermissions { get; private set; } = [];

        public WorkProfile(int id, string name, WorkProfileType workProfileType)
        {
            Id = id;
            Name = name;
            WorkProfileType = workProfileType;
        }
    }
}