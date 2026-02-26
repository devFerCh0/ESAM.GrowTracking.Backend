namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections
{
    public record LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}