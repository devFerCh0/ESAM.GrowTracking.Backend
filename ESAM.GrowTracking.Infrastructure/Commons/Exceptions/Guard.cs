namespace ESAM.GrowTracking.Infrastructure.Commons.Exceptions
{
    public static class Guard
    {
        public static void Against(bool condition, string message)
        {
            if (condition)
                throw new InfrastructureException(message);
        }

        public static void AgainstNull(object? value, string message)
        {
            if (value is null)
                throw new InfrastructureException(message);
        }

        public static void AgainstNullOrWhiteSpace(string? value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InfrastructureException(message);
        }
    }
}