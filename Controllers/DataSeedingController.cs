using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataSeedingController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DataSeedingController> _logger;

        public DataSeedingController(IServiceProvider services, ILogger<DataSeedingController> logger)
        {
            _services = services;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("seed-dummy-users")]
        public async Task<IActionResult> SeedDummyUsers()
        {
            try
            {
                await IdentityUserSeeder.SeedUsersAsync(_services);
                TempData["Success"] = "Dummy users have been seeded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding dummy users");
                TempData["Error"] = $"Error seeding users: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost("seed-dummy-claims")]
        public async Task<IActionResult> SeedDummyClaims()
        {
            try
            {
                await ClaimSeeder.SeedClaimsMaster(_services);
                TempData["Success"] = "Dummy claims have been seeded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding dummy claims");
                TempData["Error"] = $"Error seeding claims: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost("seed-departments")]
        public async Task<IActionResult> SeedDepartments()
        {
            try
            {
                await DepartmentSeeder.SeedDepartmentsAsync(_services);
                TempData["Success"] = "Departments have been seeded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding departments");
                TempData["Error"] = $"Error seeding departments: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost("seed-locations")]
        public async Task<IActionResult> SeedLocations()
        {
            try
            {
                await LocationSeeder.SeedLocationsAsync(_services);
                TempData["Success"] = "Locations have been seeded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding locations");
                TempData["Error"] = $"Error seeding locations: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}
