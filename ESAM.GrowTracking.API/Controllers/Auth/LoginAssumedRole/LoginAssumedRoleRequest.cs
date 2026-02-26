namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole
{
    public record LoginAssumedRoleRequest
    {
        public int? WorkProfileId { get; init; }

        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public LoginAssumedRoleRequest(int? workProfileId, int? roleId, int? campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}