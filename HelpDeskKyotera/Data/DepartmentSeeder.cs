using HelpDeskKyotera.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Data
{
    public static class DepartmentSeeder
    {
        public static async Task SeedDepartmentsAsync(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if departments already exist
                if (context.Departments.Any())
                {
                    Console.WriteLine("Departments table already seeded. Skipping...");
                    return;
                }

                // Ensure we have at least one location
                Location? mainLocation = await context.Locations.FirstOrDefaultAsync();
                
                if (mainLocation == null)
                {
                    // Create a default location if none exists
                    mainLocation = new Location
                    {
                        LocationId = Guid.NewGuid(),
                        Name = "Main Office",
                        Building = "Admin Building",
                        Floor = "Ground Floor",
                        City = "Kyotera",
                        Country = "Uganda"
                    };
                    await context.Locations.AddAsync(mainLocation);
                    await context.SaveChangesAsync();
                }

                var departments = new List<Department>
                {
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Information & Communication Technology",
                        Description = "Responsible for all IT infrastructure, network management, software development, and technical support",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Human Resource",
                        Description = "Handles recruitment, employee relations, training and development, payroll, and performance management",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Accounts",
                        Description = "Manages financial records, accounting operations, budgeting, and financial reporting",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Procurement",
                        Description = "Responsible for sourcing, purchasing, vendor management, and supply chain operations",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Administration",
                        Description = "Handles administrative operations, office management, facilities, and general coordination",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Operations",
                        Description = "Manages day-to-day operational activities and process optimization",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Marketing & Communications",
                        Description = "Responsible for marketing strategy, brand management, communications, and customer engagement",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Research & Development",
                        Description = "Focuses on innovation, product development, and research initiatives",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Quality Assurance",
                        Description = "Ensures quality standards, testing, compliance, and continuous improvement",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    },
                    new()
                    {
                        DepartmentId = Guid.NewGuid(),
                        Name = "Customer Support",
                        Description = "Provides customer service, technical support, and customer relationship management",
                        LocationId = mainLocation.LocationId,
                        HeadOfDepartmentId = null
                    }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();

                Console.WriteLine("Departments seeded successfully with 10 departments.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding departments: {ex.Message}");
                throw;
            }
        }
    }
}
