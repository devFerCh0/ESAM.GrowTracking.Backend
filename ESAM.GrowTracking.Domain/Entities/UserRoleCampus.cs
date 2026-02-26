using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserRoleCampus : AuditableEntity
    {
        private UserRoleCampus() { }

        public int UserId { get; private set; }

        public int RoleId { get; private set; }

        public int CampusId { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public Role Role { get; private set; } = null!;

        public Campus Campus { get; private set; } = null!;

        public List<UserSessionUserWorkProfileSelectedUserRoleCampusSelected> UserSessionUserWorkProfileSelectedUserRoleCampusSelected { get; private set; } = null!;

        public UserRoleCampus(int userId, int roleId, int campusId, int createdBy)
        {
            UserId = userId;
            RoleId = roleId;
            CampusId = campusId;
            CreateAudit(createdBy);
        }
    }
}