namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedResponse LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedResponse(int workProfileIdSelected, 
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedResponse loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected = loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected;
        }
    }
}