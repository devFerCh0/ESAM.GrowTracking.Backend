namespace ESAM.GrowTracking.Domain.Entities
{
    public class UserSessionUserWorkProfileSelectedUserRoleCampusSelected
    {
        private UserSessionUserWorkProfileSelectedUserRoleCampusSelected() { }

        public int UserSessionId { get; private set; }

        public int UserId { get; private set; }

        public int RoleId { get; private set; }

        public int CampusId { get; private set; }

        public UserSessionUserWorkProfileSelected UserSessionUserWorkProfileSelected { get; private set; } = null!;

        public UserRoleCampus UserRoleCampus { get; private set; } = null!;

        public UserSessionUserWorkProfileSelectedUserRoleCampusSelected(int userId, int roleId, int campusId)
        {
            UserId = userId;
            RoleId = roleId;
            CampusId = campusId;
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }
}