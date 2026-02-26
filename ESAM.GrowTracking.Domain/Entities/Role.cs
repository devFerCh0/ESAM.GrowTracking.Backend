using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class Role : AuditableEntity, IEntity<int>
    {
        private Role() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public List<RolePermission> RolePermissions { get; private set; } = [];

        public List<UserRoleCampus> UserRoleCampuses { get; private set; } = [];

        public Role(int id, string name, int createdBy)
        {
            Id = id;
            Name = name;
            CreateAudit(createdBy);
        }
    }
}