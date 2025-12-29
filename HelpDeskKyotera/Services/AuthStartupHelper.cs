namespace HelpDeskKyotera.Services
{
    // Simple application-scoped helper for one-time auth clearing
    public static class AuthStartupHelper
    {
        // Set to true once we've cleared the auth cookie on first request
        public static bool Cleared { get; set; } = false;
    }
}
