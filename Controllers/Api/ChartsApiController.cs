using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HelpDeskKyotera.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Staff,CEO")]
public class ChartsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChartsController(ApplicationDbContext db, IMemoryCache cache, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _cache = cache;
        _userManager = userManager;
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> TicketsMetrics()
    {
        // Cache key depends on role/user so Staff get per-user view
        var user = await _userManager.GetUserAsync(User!);
        var roles = await _userManager.GetRolesAsync(user!);
        var isStaff = roles.Contains("Staff");

        var cacheKey = isStaff && user != null ? $"charts:metrics:user:{user.Id}" : "charts:metrics:global";

        if (_cache.TryGetValue(cacheKey, out object cached))
        {
            return Ok(cached);
        }

        // Base tickets query
        var ticketsQuery = _db.Tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.Team)
            .Include(t => t.Department)
            .Include(t => t.Requester).ThenInclude(r => r.Department)
            .AsQueryable();

        // Apply Staff-scoped filter: assigned to user, or team lead, or same department
        if (isStaff && user != null)
        {
            var deptId = user.DepartmentId;
            ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == user.Id
                                                  || (t.Team != null && t.Team.TeamLeadId == user.Id)
                                                  || (t.DepartmentId != null && deptId != null && t.DepartmentId == deptId)
                                                  || (t.Requester != null && t.Requester.DepartmentId != null && deptId != null && t.Requester.DepartmentId == deptId));
        }

        // Tickets by status
        var byStatus = await ticketsQuery
            .GroupBy(t => t.Status != null ? t.Status.Name : "Unknown")
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        // Tickets by department (use ticket.Department or requester's department)
        var byDept = await ticketsQuery
            .Select(t => new { Dept = t.Department != null ? t.Department.Name : (t.Requester != null && t.Requester.Department != null ? t.Requester.Department.Name : "Unknown") })
            .GroupBy(x => x.Dept)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToListAsync();

        // SLA breaches: tickets where CreatedOn + Priority.ResolutionSLA hours < now and status is not final
        var now = DateTime.UtcNow;
        var slaBreachesCount = await ticketsQuery
            .Where(t => t.Priority != null && (t.CreatedOn.AddHours(t.Priority.ResolutionSLA) < now) && (t.Status == null || !t.Status.IsFinal))
            .CountAsync();

        var result = new
        {
            TicketsByStatus = byStatus,
            TicketsByDepartment = byDept,
            SlaBreaches = new { Count = slaBreachesCount }
        };

        // Cache for short duration
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(30))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, result, cacheEntryOptions);

        return Ok(result);
    }
}
