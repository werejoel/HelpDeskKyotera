using HelpDeskKyotera.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Data
{
    public static class LocationSeeder
    {
        public static async Task SeedLocationsAsync(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if locations already exist
                if (context.Locations.Any())
                {
                    Console.WriteLine("Locations table already seeded. Skipping...");
                    return;
                }

                var locations = new List<Location>
                {
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 1",
                        Building = "Main Building",
                        Floor = "Ground Floor",
                        Room = "Level 1",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 2",
                        Building = "Main Building",
                        Floor = "First Floor",
                        Room = "Level 2",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 3",
                        Building = "Main Building",
                        Floor = "Second Floor",
                        Room = "Level 3",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 4",
                        Building = "Main Building",
                        Floor = "Third Floor",
                        Room = "Level 4",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 5",
                        Building = "Admin Building",
                        Floor = "First Floor",
                        Room = "Level 5",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 6",
                        Building = "Admin Building",
                        Floor = "Second Floor",
                        Room = "Level 6",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 7",
                        Building = "Operations Building",
                        Floor = "Ground Floor",
                        Room = "Level 7",
                        City = "Kyotera",
                        Country = "Uganda"
                    },
                    new()
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Level 8",
                        Building = "Operations Building",
                        Floor = "First Floor",
                        Room = "Level 8",
                        City = "Kyotera",
                        Country = "Uganda"
                    }
                };

                await context.Locations.AddRangeAsync(locations);
                await context.SaveChangesAsync();

                Console.WriteLine("Locations seeded successfully with 8 levels.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding locations: {ex.Message}");
                throw;
            }
        }
    }
}
