using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserSessionUserWorkProfileSelected : AuditableEntity
    {
        public UserSessionUserWorkProfileSelected() { }

        public int UserSessionId { get; private set; }

        public int UserId { get; private set; }

        public int WorkProfileId { get; private set; }

        public UserSession UserSession { get; private set; } = null!;

        public UserWorkProfile UserWorkProfile { get; private set; } = null!;

        public UserSessionUserWorkProfileSelectedUserRoleCampusSelected UserSessionUserWorkProfileSelectedUserRoleCampusSelected { get; private set; } = null!;

        public UserSessionUserWorkProfileSelected(int userId, int workProfileId)
        {
            UserId = userId;
            WorkProfileId = workProfileId;
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }
}