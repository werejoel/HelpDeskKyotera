using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Identity;
namespace HelpDeskKyotera.Models.Data
{
    public static class IdentityUserSeeder
    {
        // Adjust this to your policy (must satisfy your password rules)
        private const string DefaultPassword = "Test@1234";
        // Predefined dummy users for each role
        private static readonly Dictionary<string, List<(string FirstName, string LastName, string Phone)>> RoleUsers =
        new()
        {
            ["Admin"] = new List<(string, string, string)>
        {
                ("Pranav", "Sharma", "9876543210"),
                ("Aditi", "Verma", "9123456789"),
                ("Rohan", "Iyer", "9988776655")
                
        },
            ["Staff"] = new List<(string, string, string)>
        {
                ("Arjun", "Mehta", "9823456780"),
                ("Sneha", "Kapoor", "9345678901"),
                ("Vikram", "Patel", "9456789012")
                
        },
            ["User"] = new List<(string, string, string)>
        {
                ("Rahul", "Singh", "9789012345"),
                ("Priya", "Menon", "9890123456"),
                ("Siddharth", "Joshi", "9901234567")
               
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
                    var email = $"{firstName.ToLower()}.{lastName.ToLower()}@dotnettutorials.net";
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