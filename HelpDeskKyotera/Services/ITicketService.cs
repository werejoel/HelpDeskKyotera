using HelpDeskKyotera.Models;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Tickets;

namespace HelpDeskKyotera.Services
{
    public interface ITicketService
    {
        // CRUD Operations
        Task<(bool Success, string Message, Guid? TicketId)> CreateTicketAsync(TicketCreateViewModel model, Guid requesterId);
        Task<TicketDetailsViewModel?> GetTicketDetailsAsync(Guid ticketId);
        Task<(bool Success, string Message)> UpdateTicketAsync(TicketEditViewModel model);
        Task<bool> DeleteTicketAsync(Guid ticketId);

        // Queries
        Task<PagedResult<TicketListItemViewModel>> GetTicketsAsync(TicketFilterViewModel filter);
        Task<PagedResult<TicketListItemViewModel>> GetMyTicketsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<TicketListItemViewModel>> GetMyAssignedTicketsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<TicketListItemViewModel>> GetDepartmentTicketsAsync(Guid departmentId, int pageNumber = 1, int pageSize = 10);

        // Status Management
        Task<(bool Success, string Message)> AssignTicketAsync(Guid ticketId, Guid? assignedToId);
        Task<(bool Success, string Message)> UpdateStatusAsync(Guid ticketId, Guid statusId);
        Task<(bool Success, string Message)> ResolveTicketAsync(Guid ticketId);
        Task<(bool Success, string Message)> ReopenTicketAsync(Guid ticketId);

        // Dropdowns
        Task<IEnumerable<StatusDropdownViewModel>> GetStatusesAsync();
        Task<IEnumerable<PriorityDropdownViewModel>> GetPrioritiesAsync();
        Task<IEnumerable<CategoryDropdownViewModel>> GetCategoriesAsync();

        // Ticket Number Generation
        public string GenerateTicketNumber();
    }
}
