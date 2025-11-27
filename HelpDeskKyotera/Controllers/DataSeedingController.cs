using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models.Data;
using Microsoft.AspNetCore.Mvc;


namespace HelpDeskKyotera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeedingController : ControllerBase
    {
        private readonly IServiceProvider _services;

        public DataSeedingController(IServiceProvider services)
        {
            _services = services;
        }

        [HttpPost("seed-dummy-users")]
        public async Task<IActionResult> SeedDummyUsers()
        {
            await IdentityUserSeeder.SeedUsersAsync(_services);
            return Ok("Dummy users have been seeded successfully.");
        }

        [HttpPost("seed-dummy-claims")]
        public async Task<IActionResult> SeedDummyClaims()
        {
            await ClaimSeeder.SeedClaimsMaster(_services);
            return Ok("Dummy claims have been seeded successfully.");
        }
    }
}
