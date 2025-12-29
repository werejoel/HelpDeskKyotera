using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;

namespace HelpDeskKyotera.Data
{
    public static class ClaimSeeder
    {
        public static async Task SeedClaimsMaster(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (context.ClaimMasters.Any())
                {
                    Console.WriteLine("ClaimMasters table already seeded. Skipping...");
                    return;
                }

                var claims = new List<ClaimMaster>
                {
                    // User-level only
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "AddUser", Category = "User", Description = "Can create new users" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "EditUser", Category = "User", Description = "Can edit users" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "DeleteUser", Category = "User", Description = "Can delete users" },

                    // Role-level only
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "AddRole", Category = "Role", Description = "Can create new roles" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "EditRole", Category = "Role", Description = "Can edit roles" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "DeleteRole", Category = "Role", Description = "Can delete roles" },

                    // Shared claims (Both)
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "ViewUsers", Category = "Both", Description = "Can view users list" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "ViewRoles", Category = "Both", Description = "Can view roles list" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "ExportReports", Category="Both", Description = "Can export reports to CSV/PDF" },
                    new() { Id = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "ManageClaims", Category="Both", Description = "Can assign and revoke claims" }
                };

                // Prevent accidental duplicates
                var distinctClaims = claims
                    .GroupBy(c => c.ClaimValue)
                    .Select(g => g.First())
                    .ToList();

                await context.ClaimMasters.AddRangeAsync(distinctClaims);
                await context.SaveChangesAsync();

                Console.WriteLine("ClaimMasters seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while seeding ClaimMasters: {ex.Message}");
            }
        }
    }
}