namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel
    {

        public int RoleIdSelected { get; init; }

        public int CampusIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel(int roleIdSelected, int campusIdSelected)
        {
            RoleIdSelected = roleIdSelected;
            CampusIdSelected = campusIdSelected;
        }
    }
}