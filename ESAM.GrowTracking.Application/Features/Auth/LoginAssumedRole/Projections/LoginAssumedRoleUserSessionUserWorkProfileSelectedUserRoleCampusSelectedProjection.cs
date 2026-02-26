namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection
    {
        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}