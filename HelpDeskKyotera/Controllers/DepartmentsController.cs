using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.Models;

namespace HelpDeskKyotera.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return View(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading departments");
                TempData["Error"] = "An error occurred while loading departments.";
                return View(new List<Department>());
            }
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
                if (department == null)
                    return NotFound();

                var users = await _departmentService.GetDepartmentUsersAsync(id.Value);
                ViewBag.Users = users;

                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading department {id}");
                TempData["Error"] = "An error occurred while loading the department.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Departments/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var allUsers = await Task.FromResult(new List<ApplicationUser>());
                ViewBag.HeadOfDepartmentId = new SelectList(allUsers, "Id", "FullName");
                
                // Load locations for dropdown
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description, Guid? headId, Guid? locationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Department name is required.");
                
                // Reload locations for dropdown on error
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name");
                
                return View();
            }

            try
            {
                var (success, message, departmentId) = await _departmentService.CreateDepartmentAsync(name, description, headId, locationId);
                
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction(nameof(Details), new { id = departmentId });
                }

                TempData["Error"] = message;
                
                // Reload locations for dropdown on error
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                TempData["Error"] = "An unexpected error occurred.";
                return View();
            }
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
                if (department == null)
                    return NotFound();

                var allUsers = new List<ApplicationUser>();
                ViewBag.HeadOfDepartmentId = new SelectList(allUsers, "Id", "FullName", department.HeadOfDepartmentId);
                
                // Load locations for dropdown
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name", department.LocationId);

                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for department {id}");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string name, string? description, Guid? headId, Guid? locationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Department name is required.");
                
                // Reload locations for dropdown on error
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name");
                
                return View();
            }

            try
            {
                var (success, message) = await _departmentService.UpdateDepartmentAsync(id, name, description, headId, locationId);

                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Error"] = message;
                
                // Reload locations for dropdown on error
                var locations = await _departmentService.GetAllLocationsAsync();
                ViewBag.LocationId = new SelectList(locations, "LocationId", "Name");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating department {id}");
                TempData["Error"] = "An unexpected error occurred.";
                return View();
            }
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
                if (department == null)
                    return NotFound();

                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for {id}");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var success = await _departmentService.DeleteDepartmentAsync(id);

                if (success)
                {
                    TempData["Success"] = "Department deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Cannot delete department with users or tickets assigned.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting department {id}");
                TempData["Error"] = "An error occurred while deleting the department.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Departments/AssignUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUser(Guid departmentId, Guid userId)
        {
            try
            {
                var (success, message) = await _departmentService.AssignUserToDepartmentAsync(userId, departmentId);

                if (success)
                    TempData["Success"] = message;
                else
                    TempData["Error"] = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning user {userId} to department {departmentId}");
                TempData["Error"] = "An error occurred while assigning the user.";
            }

            return RedirectToAction(nameof(Details), new { id = departmentId });
        }

        // GET: Departments/Tickets/5
        public async Task<IActionResult> Tickets(Guid? id, int pageNumber = 1)
        {
            if (id == null)
                return NotFound();

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
                if (department == null)
                    return NotFound();

                var tickets = await _departmentService.GetDepartmentTicketsAsync(id.Value, pageNumber, 10);
                ViewBag.Department = department;

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading tickets for department {id}");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
