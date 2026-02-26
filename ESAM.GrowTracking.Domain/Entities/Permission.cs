using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class Permission : IEntity<int>
    {
        private Permission() { }

        public int Id { get; private set; }

        public int ModuleId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public string? Code { get; private set; }

        public bool IsDeleted { get; private set; }

        public Module Module { get; private set; } = null!;

        public List<RolePermission> RolePermissions { get; private set; } = [];

        public List<WorkProfilePermission> WorkProfilePermissions { get; private set; } = [];

        public Permission(int id, int moduleId, string name)
        {
            Id = id;
            ModuleId = moduleId;
            Name = name;
        }
    }
}