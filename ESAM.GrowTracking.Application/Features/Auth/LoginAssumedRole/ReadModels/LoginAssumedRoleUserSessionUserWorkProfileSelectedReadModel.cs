namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel(int workProfileIdSelected, 
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
            LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected = loginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelected;
        }
    }
}