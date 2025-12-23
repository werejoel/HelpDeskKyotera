using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            // Register Entity Framework Core with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString)
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            // Register ASP.NET Core Identity Services using AddIdentity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
                    options =>
                    {
                        // Password settings
                        options.Password.RequireDigit = true;               // Must include digits
                        options.Password.RequiredLength = 8;                // Minimum length 8
                        options.Password.RequireNonAlphanumeric = true;     // Must include special characters
                        options.Password.RequireUppercase = true;           // Must include uppercase letters
                        options.Password.RequireLowercase = true;           // Must include lowercase letters
                        options.Password.RequiredUniqueChars = 4;           // At least 4 unique characters
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            // Set token valid for 30 minutes
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(30);
            });



            // Configure the Application Cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
                options.LoginPath = "/Account/Login"; // Set your login path here

                // If the AccessDenied isn't set, ASP.NET Core defaults the path to /Account/AccessDenied
                options.AccessDeniedPath = "/Account/AccessDenied"; // Set your access denied path here
            });




            // Registering Custom Services
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IClaimsService, ClaimsService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add Authentication and Authorization Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}