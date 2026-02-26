namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection? LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection(int workProfileIdSelected,
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection? loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected = loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected;
        }
    }
}