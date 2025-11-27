using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename tables
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            builder.Entity<ClaimMaster>().ToTable("ClaimMasters");




            // Seed initial roles using HasData
            var adminRoleId = Guid.Parse("c8d89a25-4b96-4f20-9d79-7f8a54c5213d");
            var userRoleId = Guid.Parse("b92f0a3e-573b-4b12-8db1-2ccf6d58a34a");
            var managerRoleId = Guid.Parse("d7f4a42e-1c1b-4c9f-8a50-55f6b234e8e2");
            var ceoRoleId = Guid.Parse("f2e6b8a1-9d43-4a7c-9f32-71d7c5dbe9f0");



            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator role with full permissions.",
                    IsActive = true,
                    CreatedOn = new DateTime(2025, 8, 4),
                    ModifiedOn = new DateTime(2025, 8, 4)
                },
                new ApplicationRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Standard user role.",
                    IsActive = true,
                    CreatedOn = new DateTime(2025, 8, 4),
                    ModifiedOn = new DateTime(2025, 8, 4)
                },
                new ApplicationRole
                {
                    Id = managerRoleId,
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    Description = "Staff role with moderate permissions.",
                    IsActive = true,
                    CreatedOn = new DateTime(2025, 8, 4),
                    ModifiedOn = new DateTime(2025, 8, 4)
                },
                new ApplicationRole
                {
                    Id = ceoRoleId,
                    Name = "ceo",
                    NormalizedName = "CEO",
                    Description = "Ceo role with some access.",
                    IsActive = true,
                    CreatedOn = new DateTime(2025, 8, 4),
                    ModifiedOn = new DateTime(2025, 8, 4)
                }
            );
            // Seed default admin user and user-role mapping
            SeedTmsData(builder);
        }
        // Additional seeding helpers
        private static void SeedTmsData(ModelBuilder builder)
        {
            var adminUserId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");
            var hasher = new PasswordHasher<ApplicationUser>();

            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminUserId,
                    UserName = "admin@helpdsk.com",
                    NormalizedUserName = "ADMIN@HELPDSK.COM",
                    Email = "admin@helpdsk.com",
                    NormalizedEmail = "ADMIN@HELPDSK.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "Admin@123"),
                    SecurityStamp = "admin-security-stamp-fixed-value",
                    FirstName = "System",
                    LastName = "Administrator",
                    PhoneNumber = "+256700000000",
                    IsActive = true,
                    CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    RoleId = Guid.Parse("c8d89a25-4b96-4f20-9d79-7f8a54c5213d"),
                    UserId = adminUserId
                }
            );
        }
          // DbSets
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ClaimMaster> ClaimMasters { get; set; }
    }
}