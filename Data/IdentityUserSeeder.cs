using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Identity;
namespace HelpDeskKyotera.Models.Data
{
    public static class IdentityUserSeeder
    {
        // Adjust this to your policy
        private const string DefaultPassword = "Test@1234";
        // Predefined dummy users for each role
        private static readonly Dictionary<string, List<(string FirstName, string LastName, string Phone)>> RoleUsers =
        new()
        {
            ["Admin"] = new List<(string, string, string)>
        {
                ("Najuko ", "Prossy", "0789251487"),
                ("Tumwinne ", "Derrick", "0789251433"),
                
        },
            ["Staff"] = new List<(string, string, string)>
        {
               ("Namaganda ", "Jane", "0789251357"),
                ("Okiring ", "Samuel", "0789251425"),
               
        },
            ["User"] = new List<(string, string, string)>
        {
                ("Kapoto", "Simon", "0789251425"),
                ("Kintu", "Viscent", "0789253546"),
                
               
        }
        };
        public static async Task SeedUsersAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var role in RoleUsers.Keys)
            {
                foreach (var (firstName, lastName, phone) in RoleUsers[role])
                {
                    var email = $"{firstName.ToLower()}.{lastName.ToLower()}";
                    await EnsureUserInRoleAsync(userManager, firstName, lastName, email, phone, role, DefaultPassword);
                }
            }
        }
        private static async Task EnsureUserInRoleAsync(
        UserManager<ApplicationUser> userManager,
        string firstName,
        string lastName,
        string email,
        string phone,
        string role,
        string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    // Optional: log error
                    return;
                }
            }
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}