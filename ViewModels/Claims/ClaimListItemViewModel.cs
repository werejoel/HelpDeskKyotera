namespace HelpDeskKyotera.ViewModels.Claims
{
    public class ClaimListItemViewModel
    {
        public Guid Id { get; init; }
        public string ClaimType { get; init; } = string.Empty;
        public string ClaimValue { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty; // User, Role, Both
        public string? Description { get; init; }
        public bool IsActive { get; init; }
    }
}