namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels
{
    public record LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}