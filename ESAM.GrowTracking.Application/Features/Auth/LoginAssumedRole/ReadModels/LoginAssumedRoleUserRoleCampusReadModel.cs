namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleUserRoleCampusReadModel
    {
        public int RoleId { get; init; }

        public string Role { get; init; }

        public int CampusId { get; init; }

        public string Campus { get; init; }

        public LoginAssumedRoleUserRoleCampusReadModel(int roleId, string role, int campusId, string campus)
        {
            RoleId = roleId;
            Role = role;
            CampusId = campusId;
            Campus = campus;
        }
    }
}