namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedResponse
    {

        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedResponse(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}