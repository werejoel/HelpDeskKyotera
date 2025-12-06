using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HelpDeskKyotera.Models;
namespace HelpDeskKyotera.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //  DbSets properties
    public DbSet<Department> Departments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<ClaimMaster> ClaimMasters { get; set; }
    //public DbSet<Article> Articles { get; set; }
    //public DbSet<CannedResponse> CannedResponses { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ==================== IDENTITY TABLE RENAMING ====================
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        
        // RELATIONSHIPS 
        // User → Department (optional)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)  // now Guid?
            .OnDelete(DeleteBehavior.SetNull);

        // User → Location (optional)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Location)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.LocationId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.Entity<ClaimMaster>()
            .ToTable("ClaimMasters");

        builder.Entity<ClaimMaster>()
            .HasIndex(c => c.ClaimValue)
            .IsUnique();

        

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Location)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.LocationId)    // now Guid?
            .OnDelete(DeleteBehavior.SetNull);

        // Department → Location
        builder.Entity<Department>()
            .HasOne(d => d.Location)
            .WithMany(l => l.Departments)
            .HasForeignKey(d => d.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Team → TeamLead
        builder.Entity<Team>()
            .HasOne(t => t.TeamLead)
            .WithMany()
            .HasForeignKey(t => t.TeamLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ticket Relationships
        builder.Entity<Ticket>()
            .HasOne(t => t.Requester)
            .WithMany(u => u.RequestedTickets)
            .HasForeignKey(t => t.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Ticket>()
            .HasOne(t => t.Team)
            .WithMany(tm => tm.Tickets)
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Ticket>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Priority)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Status)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category Hierarchy (Self-reference)
        builder.Entity<Category>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasOne(c => c.DefaultTeam)
            .WithMany()
            .HasForeignKey(c => c.DefaultTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Comment → Author & Ticket
        builder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attachment → UploadedBy
        builder.Entity<Attachment>()
            .HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== INDEXES FOR PERFORMANCE ====================
        builder.Entity<Ticket>()
            .HasIndex(t => t.TicketNumber)
            .IsUnique();

        builder.Entity<Ticket>()
            .HasIndex(t => t.CreatedOn);

        builder.Entity<Ticket>()
            .HasIndex(t => t.StatusId);

        builder.Entity<Ticket>()
            .HasIndex(t => t.RequesterId);

        builder.Entity<Ticket>()
            .HasIndex(t => t.AssignedToId);

        // ==================== SEED DATA ====================
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder b)
    {
        var hasher = new PasswordHasher<ApplicationUser>();

        // === 1. Roles ===
        var adminRoleId = Guid.Parse("c8d89a25-4b96-4f20-9d79-7f8a54c5213d");
        var staffRoleId = Guid.Parse("d7f4a42e-1c1b-4c9f-8a50-55f6b234e8e2");
        var userRoleId = Guid.Parse("b92f0a3e-573b-4b12-8db1-2ccf6d58a34a");
        var ceoRoleId = Guid.Parse("f2e6b8a1-9d43-4a7c-9f32-71d7c5dbe9f0");

        b.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", Description = "Full system access", CreatedOn = new DateTime(2025, 1, 1) },
            new ApplicationRole { Id = staffRoleId, Name = "Staff", NormalizedName = "STAFF", Description = "IT Support Staff", CreatedOn = new DateTime(2025, 1, 1) },
            new ApplicationRole { Id = userRoleId, Name = "User", NormalizedName = "USER", Description = "End User", CreatedOn = new DateTime(2025, 1, 1) },
            new ApplicationRole { Id = ceoRoleId, Name = "CEO", NormalizedName = "CEO", Description = "Executive Access", CreatedOn = new DateTime(2025, 1, 1) }
        );

        // === 2. Admin User ===
        var adminUserId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");
        b.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@helpdeskyotera.com",
                NormalizedUserName = "ADMIN@HELPDESKYOTERA.COM",
                Email = "admin@helpdeskyotera.com",
                NormalizedEmail = "ADMIN@HELPDESKYOTERA.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null!, "Admin@2025"),
                SecurityStamp = "ADMINSECURITYSTAMP123",
                ConcurrencyStamp = "admin-concurrency",
                FirstName = "System",
                LastName = "Administrator",
                PhoneNumber = "+256781234567",
                IsActive = true,
                CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        b.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid> { UserId = adminUserId, RoleId = adminRoleId }
        );

        // === 3. Locations ===
        var locMain = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var locIt = Guid.Parse("22222222-2222-2222-2222-222222222222");
        b.Entity<Location>().HasData(
            new Location { LocationId = locMain, Name = "Main Office Kyotera", Building = "Head Office", City = "Kyotera", Country = "Uganda" },
            new Location { LocationId = locIt, Name = "IT Server Room", Building = "Head Office", Floor = "2nd", Room = "IT-201", City = "Kyotera", Country = "Uganda" }
        );

        // === 4. Priorities ===
        var pLow = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var pMed = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var pHigh = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var pCrit = Guid.Parse("10000000-0000-0000-0000-000000000004");

        b.Entity<Priority>().HasData(
            new Priority { PriorityId = pLow, Name = "Low", ResponseSLA = 48, ResolutionSLA = 240, Order = 1 },
            new Priority { PriorityId = pMed, Name = "Medium", ResponseSLA = 8, ResolutionSLA = 72, Order = 2 },
            new Priority { PriorityId = pHigh, Name = "High", ResponseSLA = 4, ResolutionSLA = 24, Order = 3 },
            new Priority { PriorityId = pCrit, Name = "Critical", ResponseSLA = 1, ResolutionSLA = 8, Order = 4 }
        );

        // === 5. Statuses ===
        var sOpen = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var sProg = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var sPend = Guid.Parse("20000000-0000-0000-0000-000000000003");
        var sRes = Guid.Parse("20000000-0000-0000-0000-000000000004");
        var sClosed = Guid.Parse("20000000-0000-0000-0000-000000000005");

        b.Entity<Status>().HasData(
            new Status { StatusId = sOpen, Name = "Open", IsFinal = false, Order = 1 },
            new Status { StatusId = sProg, Name = "In Progress", IsFinal = false, Order = 2 },
            new Status { StatusId = sPend, Name = "Pending", IsFinal = false, Order = 3 },
            new Status { StatusId = sRes, Name = "Resolved", IsFinal = true, Order = 4 },
            new Status { StatusId = sClosed, Name = "Closed", IsFinal = true, Order = 5 }
        );

        // === 6. Teams ===
        var teamDesktop = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var teamNetwork = Guid.Parse("30000000-0000-0000-0000-000000000002");

        b.Entity<Team>().HasData(
            new Team { TeamId = teamDesktop, Name = "Desktop Support", Specialization = "Hardware & Desktop", TeamLeadId = null },
            new Team { TeamId = teamNetwork, Name = "Network Team", Specialization = "Network & Internet", TeamLeadId = null }
        );

        // === 7. Categories (with hierarchy) ===
        var catHardware = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var catSoftware = Guid.Parse("40000000-0000-0000-0000-000000000002");
        var subLaptop = Guid.Parse("40000000-0000-0000-0000-000000000003");

        b.Entity<Category>().HasData(
            new Category { CategoryId = catHardware, Name = "Hardware", DefaultTeamId = teamDesktop },
            new Category { CategoryId = catSoftware, Name = "Software", DefaultTeamId = teamDesktop },
            new Category { CategoryId = subLaptop, Name = "Laptop Issues", ParentId = catHardware, DefaultTeamId = teamDesktop }
        );
    }
}