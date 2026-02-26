using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserWorkProfile : AuditableEntity
    {
        private UserWorkProfile() { }

        public int UserId { get; private set; }

        public int WorkProfileId { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public WorkProfile WorkProfile { get; private set; } = null!;

        public List<UserSessionUserWorkProfileSelected> UserSessionUserWorkProfilesSelected {  get; private set; } = [];

        public UserWorkProfile(int userId, int workProfileId, int createdBy)
        {
            UserId = userId;
            WorkProfileId = workProfileId;
            CreateAudit(createdBy);
        }
    }
}