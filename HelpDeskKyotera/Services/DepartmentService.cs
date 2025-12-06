using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Department?> GetDepartmentByIdAsync(Guid departmentId)
        {
            try
            {
                return await _context.Departments
                    .Include(d => d.Head)
                    .Include(d => d.Location)
                    .Include(d => d.Users)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving department {departmentId}");
                return null;
            }
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            try
            {
                return await _context.Departments
                    .Include(d => d.Head)
                    .Include(d => d.Location)
                    .Include(d => d.Users)
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all departments");
                return Enumerable.Empty<Department>();
            }
        }

        public async Task<(bool Success, string Message, Guid? DepartmentId)> CreateDepartmentAsync(string name, string? description, Guid? headId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return (false, "Department name is required.", null);

                // Check if department already exists
                var exists = await _context.Departments.AnyAsync(d => d.Name == name.Trim());
                if (exists)
                    return (false, "A department with this name already exists.", null);

                var department = new Department
                {
                    DepartmentId = Guid.NewGuid(),
                    Name = name.Trim(),
                    Description = description?.Trim(),
                    HeadOfDepartmentId = headId
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Department '{name}' created successfully");
                return (true, $"Department '{name}' created successfully.", department.DepartmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return (false, "An error occurred while creating the department.", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(Guid departmentId, string name, string? description, Guid? headId)
        {
            try
            {
                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null)
                    return (false, "Department not found.");

                if (string.IsNullOrWhiteSpace(name))
                    return (false, "Department name is required.");

                // Check for duplicate name (excluding current)
                var duplicate = await _context.Departments
                    .AnyAsync(d => d.DepartmentId != departmentId && d.Name == name.Trim());
                if (duplicate)
                    return (false, "A department with this name already exists.");

                department.Name = name.Trim();
                department.Description = description?.Trim();
                department.HeadOfDepartmentId = headId;

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Department '{name}' updated successfully");
                return (true, "Department updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating department {departmentId}");
                return (false, "An error occurred while updating the department.");
            }
        }

        public async Task<bool> DeleteDepartmentAsync(Guid departmentId)
        {
            try
            {
                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null)
                    return false;

                // Check if department has users or tickets
                var hasUsers = await _context.Users.AnyAsync(u => u.DepartmentId == departmentId);
                var hasTickets = await _context.Tickets.AnyAsync(t => t.DepartmentId == departmentId);

                if (hasUsers || hasTickets)
                {
                    _logger.LogWarning($"Cannot delete department {departmentId}: has users or tickets");
                    return false;
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Department deleted: {departmentId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting department {departmentId}");
                return false;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetDepartmentUsersAsync(Guid departmentId)
        {
            try
            {
                return await _context.Users
                    .Where(u => u.DepartmentId == departmentId && u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving users for department {departmentId}");
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        public async Task<(bool Success, string Message)> AssignUserToDepartmentAsync(Guid userId, Guid departmentId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return (false, "User not found.");

                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null)
                    return (false, "Department not found.");

                user.DepartmentId = departmentId;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {userId} assigned to department {departmentId}");
                return (true, "User assigned to department successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning user {userId} to department {departmentId}");
                return (false, "An error occurred while assigning the user.");
            }
        }

        public async Task<(bool Success, string Message)> RemoveUserFromDepartmentAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return (false, "User not found.");

                user.DepartmentId = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {userId} removed from department");
                return (true, "User removed from department successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user {userId} from department");
                return (false, "An error occurred while removing the user.");
            }
        }

        public async Task<PagedResult<dynamic>> GetDepartmentTicketsAsync(Guid departmentId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Tickets
                    .Where(t => t.DepartmentId == departmentId)
                    .Include(t => t.Status)
                    .Include(t => t.Priority)
                    .Include(t => t.Category)
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(t => t.CreatedOn)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<dynamic>
                {
                    Items = items.Cast<dynamic>().ToList().AsReadOnly(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving department tickets for {departmentId}");
                return new PagedResult<dynamic>
                {
                    Items = new List<dynamic>().AsReadOnly(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public async Task<(bool Success, string Message)> AssignTicketToDepartmentAsync(Guid ticketId, Guid departmentId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null)
                    return (false, "Department not found.");

                ticket.DepartmentId = departmentId;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticketId} assigned to department {departmentId}");
                return (true, "Ticket assigned to department successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning ticket {ticketId} to department {departmentId}");
                return (false, "An error occurred while assigning the ticket.");
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAvailableStaffForDepartmentAsync(Guid departmentId)
        {
            try
            {
                var staffRoles = new[] { "Staff", "Admin" };
                
                var departmentUsers = await _context.Users
                    .Where(u => u.DepartmentId == departmentId && u.IsActive)
                    .ToListAsync();

                var availableStaff = new List<ApplicationUser>();

                foreach (var user in departmentUsers)
                {
                    var roles = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => r.Name)
                        .ToListAsync();

                    if (roles.Any(r => staffRoles.Contains(r!)))
                    {
                        availableStaff.Add(user);
                    }
                }

                return availableStaff.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving available staff for department {departmentId}");
                return Enumerable.Empty<ApplicationUser>();
            }
        }
    }
}
