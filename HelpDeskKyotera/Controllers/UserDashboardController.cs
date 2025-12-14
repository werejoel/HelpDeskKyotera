using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;

namespace HelpDeskKyotera.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITicketService _ticketService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<UserDashboardController> _logger;

        public UserDashboardController(
            UserManager<ApplicationUser> userManager,
            ITicketService ticketService,
            IDepartmentService departmentService,
            ILogger<UserDashboardController> logger)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _departmentService = departmentService;
            _logger = logger;
        }

        // GET: /UserDashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Get user details
                var userDetails = await _userManager.FindByIdAsync(user.Id.ToString());
                if (userDetails == null)
                    return RedirectToAction("Login", "Account");

                // Get user's department
                var department = userDetails.DepartmentId.HasValue
                    ? await _departmentService.GetDepartmentByIdAsync(userDetails.DepartmentId.Value)
                    : null;

                // Get user's tickets (as requester)
                var requestedTickets = await _ticketService.GetTicketsByRequesterAsync(user.Id, pageNumber: 1, pageSize: 5);

                // Get user's assigned tickets (as assignee) if user has staff role
                var userRoles = await _userManager.GetRolesAsync(user);
                var assignedTickets = userRoles.Contains("Staff") || userRoles.Contains("Admin")
                    ? await _ticketService.GetTicketsByAssigneeAsync(user.Id, pageNumber: 1, pageSize: 5)
                    : new PagedResult<Ticket> { Items = new List<Ticket>(), TotalCount = 0, PageNumber = 1, PageSize = 5 };

                // Get statistics
                var totalRequestedTickets = requestedTickets.TotalCount;
                var totalAssignedTickets = assignedTickets.TotalCount;

                // Build view model
                var dashboardViewModel = new UserDashboardViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Department = department,
                    UserRoles = userRoles.ToList(),
                    TotalRequestedTickets = totalRequestedTickets,
                    TotalAssignedTickets = totalAssignedTickets,
                    RecentRequestedTickets = requestedTickets.Items.ToList(),
                    RecentAssignedTickets = assignedTickets.Items.ToList(),
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return View();
            }
        }

        // GET: /UserDashboard/Profile
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var userDetails = await _userManager.FindByIdAsync(user.Id.ToString());
                if (userDetails == null)
                    return RedirectToAction("Login", "Account");

                var userRoles = await _userManager.GetRolesAsync(userDetails);

                var profileViewModel = new UserProfileViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    JobTitle = userDetails.JobTitle,
                    EmployeeId = userDetails.EmployeeId,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedOn = userDetails.CreatedOn,
                    LastLogin = userDetails.LastLogin,
                    Roles = userRoles.ToList()
                };

                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["Error"] = "An error occurred while loading the profile.";
                return RedirectToAction("Index");
            }
        }

        // GET: /UserDashboard/MyTickets
        public async Task<IActionResult> MyTickets(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var tickets = await _ticketService.GetTicketsByRequesterAsync(user.Id, pageNumber, pageSize);

                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user tickets");
                TempData["Error"] = "An error occurred while loading your tickets.";
                return RedirectToAction("Index");
            }
        }

        // GET: /UserDashboard/AssignedTickets
        public async Task<IActionResult> AssignedTickets(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Staff") && !userRoles.Contains("Admin"))
                {
                    TempData["Error"] = "You do not have access to view assigned tickets.";
                    return RedirectToAction("Index");
                }

                var tickets = await _ticketService.GetTicketsByAssigneeAsync(user.Id, pageNumber, pageSize);

                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assigned tickets");
                TempData["Error"] = "An error occurred while loading assigned tickets.";
                return RedirectToAction("Index");
            }
        }
    }
}
