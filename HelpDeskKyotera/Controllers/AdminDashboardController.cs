using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Tickets;
using HelpDeskKyotera.ViewModels.Users;

namespace HelpDeskKyotera.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            IDepartmentService departmentService,
            ITicketService ticketService,
            IUserService userService,
            ILogger<AdminDashboardController> logger)
        {
            _departmentService = departmentService;
            _ticketService = ticketService;
            _userService = userService;
            _logger = logger;
        }

        // GET: AdminDashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = new AdminDashboardViewModel();

                // Get statistics
                var ticketsFilter = new TicketFilterViewModel { PageNumber = 1, PageSize = int.MaxValue };
                var pagedTickets = await _ticketService.GetTicketsAsync(ticketsFilter);
                
                var allDepartments = await _departmentService.GetAllDepartmentsAsync();
                
                var usersFilter = new UserListFilterViewModel { PageNumber = 1, PageSize = int.MaxValue };
                var pagedUsers = await _userService.GetUsersAsync(usersFilter);

                dashboardData.TotalTickets = pagedTickets?.TotalCount ?? 0;
                dashboardData.TotalDepartments = allDepartments?.Count() ?? 0;
                dashboardData.TotalUsers = pagedUsers?.TotalCount ?? 0;

                // Get recent tickets (first 5)
                if (pagedTickets?.Items != null && pagedTickets.Items.Any())
                {
                    dashboardData.RecentTickets = pagedTickets.Items.Take(5).Select(t => new Ticket
                    {
                        TicketId = t.TicketId,
                        TicketNumber = t.TicketNumber,
                        Title = t.Title,
                        CreatedOn = t.CreatedOn,
                        Status = new Status { Name = t.StatusName }
                    }).ToList();
                }

                // Get departments
                dashboardData.Departments = allDepartments?.ToList() ?? new List<Department>();

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return View(new AdminDashboardViewModel());
            }
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalTickets { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalUsers { get; set; }
        public List<Ticket> RecentTickets { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
    }
}
