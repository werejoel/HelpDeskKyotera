using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Tickets;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ApplicationDbContext context, ILogger<TicketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<TicketListItemViewModel>> GetTicketsAsync(TicketFilterViewModel filter)
        {
            try
            {
                var query = _context.Tickets
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(t => t.Title.ToLower().Contains(search) ||
                                           t.TicketNumber.ToLower().Contains(search) ||
                                           t.Description.ToLower().Contains(search));
                }

                if (filter.StatusId.HasValue)
                    query = query.Where(t => t.StatusId == filter.StatusId.Value);

                if (filter.PriorityId.HasValue)
                    query = query.Where(t => t.PriorityId == filter.PriorityId.Value);

                if (filter.CategoryId.HasValue)
                    query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

                if (filter.AssignedToId.HasValue)
                    query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);

                // Get total count
                var total = await query.CountAsync();

                // Get paginated results
                var tickets = await query
                    .OrderByDescending(t => t.CreatedOn)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(t => new TicketListItemViewModel
                    {
                        TicketId = t.TicketId,
                        TicketNumber = t.TicketNumber,
                        Title = t.Title,
                        CategoryName = t.Category.Name,
                        PriorityName = t.Priority.Name,
                        StatusName = t.Status.Name,
                        RequesterName = $"{t.Requester.FirstName} {t.Requester.LastName}".Trim(),
                        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}".Trim() : null,
                        CreatedOn = t.CreatedOn,
                        DueBy = t.DueBy
                    })
                    .ToListAsync();

                return new PagedResult<TicketListItemViewModel>
                {
                    Items = tickets,
                    TotalCount = total,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickets");
                throw;
            }
        }

        public async Task<PagedResult<TicketListItemViewModel>> GetMyAssignedTicketsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var filter = new TicketFilterViewModel
            {
                AssignedToId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return await GetTicketsAsync(filter);
        }

        public async Task<PagedResult<TicketListItemViewModel>> GetMyTicketsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Tickets
                    .Where(t => t.RequesterId == userId)
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking();

                var total = await query.CountAsync();

                var tickets = await query
                    .OrderByDescending(t => t.CreatedOn)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TicketListItemViewModel
                    {
                        TicketId = t.TicketId,
                        TicketNumber = t.TicketNumber,
                        Title = t.Title,
                        CategoryName = t.Category.Name,
                        PriorityName = t.Priority.Name,
                        StatusName = t.Status.Name,
                        RequesterName = $"{t.Requester.FirstName} {t.Requester.LastName}".Trim(),
                        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}".Trim() : null,
                        CreatedOn = t.CreatedOn,
                        DueBy = t.DueBy
                    })
                    .ToListAsync();

                return new PagedResult<TicketListItemViewModel>
                {
                    Items = tickets,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching tickets for user {userId}");
                throw;
            }
        }

        public async Task<TicketDetailsViewModel?> GetTicketDetailsAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .Include(t => t.Comments)
                    .Include(t => t.Attachments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId);

                if (ticket == null)
                    return null;

                return new TicketDetailsViewModel
                {
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    CategoryName = ticket.Category.Name,
                    PriorityName = ticket.Priority.Name,
                    StatusName = ticket.Status.Name,
                    StatusId = ticket.StatusId,
                    RequesterName = $"{ticket.Requester.FirstName} {ticket.Requester.LastName}".Trim(),
                    AssignedToName = ticket.AssignedTo != null ? $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}".Trim() : null,
                    AssignedToId = ticket.AssignedToId,
                    CreatedOn = ticket.CreatedOn,
                    DueBy = ticket.DueBy,
                    ResolvedOn = ticket.ResolvedOn,
                    ClosedOn = ticket.ClosedOn,
                    CommentCount = ticket.Comments?.Count ?? 0,
                    AttachmentCount = ticket.Attachments?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching ticket details for {ticketId}");
                throw;
            }
        }

        public async Task<(bool Success, string Message, Guid? TicketId)> CreateTicketAsync(TicketCreateViewModel model, Guid requesterId)
        {
            try
            {
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                    return (false, "Selected category does not exist.", null);

                var priority = await _context.Priorities.FindAsync(model.PriorityId);
                if (priority == null)
                    return (false, "Selected priority does not exist.", null);

                // Get initial "Open" status
                var openStatus = await _context.Statuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "open");

                if (openStatus == null)
                    return (false, "System error: Default status not found.", null);

                var ticketId = Guid.NewGuid();
                var ticketNumber = GenerateTicketNumber();

                var ticket = new Ticket
                {
                    TicketId = ticketId,
                    TicketNumber = ticketNumber,
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    PriorityId = model.PriorityId,
                    StatusId = openStatus.StatusId,
                    RequesterId = requesterId,
                    CreatedOn = DateTime.UtcNow,
                    DueBy = model.DueBy
                };

                // Auto-assign to requester's department
                var requester = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == requesterId);
                if (requester?.DepartmentId.HasValue == true)
                {
                    ticket.DepartmentId = requester.DepartmentId;
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticketNumber} created by user {requesterId}");
                return (true, $"Ticket {ticketNumber} created successfully.", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return (false, "An error occurred while creating the ticket.", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateTicketAsync(TicketEditViewModel model)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(model.TicketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                    return (false, "Selected category does not exist.");

                var priority = await _context.Priorities.FindAsync(model.PriorityId);
                if (priority == null)
                    return (false, "Selected priority does not exist.");

                ticket.Title = model.Title;
                ticket.Description = model.Description;
                ticket.CategoryId = model.CategoryId;
                ticket.PriorityId = model.PriorityId;
                ticket.DueBy = model.DueBy;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticket.TicketNumber} updated");
                return (true, "Ticket updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket");
                return (false, "An error occurred while updating the ticket.");
            }
        }

        public async Task<(bool Success, string Message)> AssignTicketAsync(Guid ticketId, Guid? assignedToId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                if (assignedToId.HasValue)
                {
                    var user = await _context.Users.FindAsync(assignedToId.Value);
                    if (user == null)
                        return (false, "Selected user does not exist.");
                }

                ticket.AssignedToId = assignedToId;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                var assignedName = assignedToId.HasValue ? "a staff member" : "unassigned";
                _logger.LogInformation($"Ticket {ticket.TicketNumber} assigned to {assignedName}");
                return (true, $"Ticket assigned successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket");
                return (false, "An error occurred while assigning the ticket.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(Guid ticketId, Guid statusId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                var status = await _context.Statuses.FindAsync(statusId);
                if (status == null)
                    return (false, "Selected status does not exist.");

                ticket.StatusId = statusId;

                // If status is final, mark as resolved/closed
                if (status.IsFinal && !ticket.ResolvedOn.HasValue)
                    ticket.ResolvedOn = DateTime.UtcNow;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticket.TicketNumber} status updated to {status.Name}");
                return (true, $"Ticket status updated to {status.Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status");
                return (false, "An error occurred while updating the ticket status.");
            }
        }

        public async Task<(bool Success, string Message)> ResolveTicketAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                var resolvedStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "resolved");

                if (resolvedStatus == null)
                    return (false, "System error: Resolved status not found.");

                ticket.StatusId = resolvedStatus.StatusId;
                ticket.ResolvedOn = DateTime.UtcNow;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticket.TicketNumber} resolved");
                return (true, "Ticket resolved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving ticket");
                return (false, "An error occurred while resolving the ticket.");
            }
        }

        public async Task<(bool Success, string Message)> ReopenTicketAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return (false, "Ticket not found.");

                var openStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "open");

                if (openStatus == null)
                    return (false, "System error: Open status not found.");

                ticket.StatusId = openStatus.StatusId;
                ticket.ResolvedOn = null;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket {ticket.TicketNumber} reopened");
                return (true, "Ticket reopened successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening ticket");
                return (false, "An error occurred while reopening the ticket.");
            }
        }

        public async Task<IEnumerable<StatusDropdownViewModel>> GetStatusesAsync()
        {
            return await _context.Statuses
                .AsNoTracking()
                .OrderBy(s => s.Order)
                .Select(s => new StatusDropdownViewModel
                {
                    StatusId = s.StatusId,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PriorityDropdownViewModel>> GetPrioritiesAsync()
        {
            return await _context.Priorities
                .AsNoTracking()
                .OrderBy(p => p.Order)
                .Select(p => new PriorityDropdownViewModel
                {
                    PriorityId = p.PriorityId,
                    Name = p.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryDropdownViewModel>> GetCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDropdownViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteTicketAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return false;

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ticket {ticketId}");
                return false;
            }
        }

        public async Task<PagedResult<TicketListItemViewModel>> GetDepartmentTicketsAsync(Guid departmentId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Tickets
                    .Where(t => t.DepartmentId == departmentId)
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();
                var tickets = await query
                    .OrderByDescending(t => t.CreatedOn)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = tickets.Select(t => new TicketListItemViewModel
                {
                    TicketId = t.TicketId,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    CategoryName = t.Category.Name,
                    PriorityName = t.Priority.Name,
                    StatusName = t.Status.Name,
                    RequesterName = t.Requester.FullName,
                    AssignedToName = t.AssignedTo?.FullName,
                    CreatedOn = t.CreatedOn,
                    DueBy = t.DueBy
                }).ToList();

                return new PagedResult<TicketListItemViewModel>
                {
                    Items = items.AsReadOnly(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving department tickets for {departmentId}");
                return new PagedResult<TicketListItemViewModel>
                {
                    Items = new List<TicketListItemViewModel>().AsReadOnly(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public string GenerateTicketNumber()
        {
            // Format: INC + year + month + day + sequence (e.g., INC20251206001)
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var lastTicket = _context.Tickets
                .Where(t => t.TicketNumber.StartsWith($"INC{date}"))
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefault();

            int sequence = 1;
            if (lastTicket != null && lastTicket.TicketNumber.Length > 12)
            {
                if (int.TryParse(lastTicket.TicketNumber.Substring(12), out int lastSeq))
                    sequence = lastSeq + 1;
            }

            return $"INC{date}{sequence:D3}";
        }

        public async Task<PagedResult<Ticket>> GetTicketsByRequesterAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Tickets
                    .Where(t => t.RequesterId == userId)
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreatedOn);

                var total = await query.CountAsync();
                var tickets = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Ticket>
                {
                    Items = tickets,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets for requester {userId}");
                return new PagedResult<Ticket> 
                { 
                    Items = new List<Ticket>(), 
                    TotalCount = 0, 
                    PageNumber = pageNumber, 
                    PageSize = pageSize 
                };
            }
        }

        public async Task<PagedResult<Ticket>> GetTicketsByAssigneeAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Tickets
                    .Where(t => t.AssignedToId == userId)
                    .Include(t => t.Requester)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreatedOn);

                var total = await query.CountAsync();
                var tickets = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Ticket>
                {
                    Items = tickets,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets for assignee {userId}");
                return new PagedResult<Ticket> 
                { 
                    Items = new List<Ticket>(), 
                    TotalCount = 0, 
                    PageNumber = pageNumber, 
                    PageSize = pageSize 
                };
            }
        }
    }
}
