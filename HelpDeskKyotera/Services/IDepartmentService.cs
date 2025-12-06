using HelpDeskKyotera.Models;
using HelpDeskKyotera.ViewModels;

namespace HelpDeskKyotera.Services
{
    public interface IDepartmentService
    {
        // Department CRUD
        Task<Department?> GetDepartmentByIdAsync(Guid departmentId);
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<(bool Success, string Message, Guid? DepartmentId)> CreateDepartmentAsync(string name, string? description, Guid? headId);
        Task<(bool Success, string Message)> UpdateDepartmentAsync(Guid departmentId, string name, string? description, Guid? headId);
        Task<bool> DeleteDepartmentAsync(Guid departmentId);

        // Department Users
        Task<IEnumerable<ApplicationUser>> GetDepartmentUsersAsync(Guid departmentId);
        Task<(bool Success, string Message)> AssignUserToDepartmentAsync(Guid userId, Guid departmentId);
        Task<(bool Success, string Message)> RemoveUserFromDepartmentAsync(Guid userId);

        // Department Tickets
        Task<PagedResult<dynamic>> GetDepartmentTicketsAsync(Guid departmentId, int pageNumber = 1, int pageSize = 10);
        Task<(bool Success, string Message)> AssignTicketToDepartmentAsync(Guid ticketId, Guid departmentId);
        Task<IEnumerable<ApplicationUser>> GetAvailableStaffForDepartmentAsync(Guid departmentId);
    }
}
