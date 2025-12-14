using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Tickets;
using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;

namespace HelpDeskKyotera.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ApplicationDbContext context, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _context = context;
            _logger = logger;
        }
        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
        // GET: Tickets/Index
        public async Task<IActionResult> Index(int pageNumber = 1, string? search = null, Guid? statusId = null, Guid? priorityId = null, Guid? categoryId = null)
        {
            try
            {
                var filter = new TicketFilterViewModel 
                { 
                    PageNumber = pageNumber, 
                    PageSize = 10,
                    Search = search,
                    StatusId = statusId,
                    PriorityId = priorityId,
                    CategoryId = categoryId
                };
                var result = await _ticketService.GetTicketsAsync(filter);

                // Load filter options for dropdowns
                var statuses = await _ticketService.GetStatusesAsync();
                var priorities = await _ticketService.GetPrioritiesAsync();
                var categories = await _ticketService.GetCategoriesAsync();
                ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");
                ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name");
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                ViewBag.SearchTerm = search;
                ViewBag.SelectedStatus = statusId;
                ViewBag.SelectedPriority = priorityId;
                ViewBag.SelectedCategory = categoryId;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tickets");
                TempData["Error"] = "An error occurred while loading tickets.";
                return View(new PagedResult<TicketListItemViewModel>());
            }
        }

        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _ticketService.GetMyTicketsAsync(userId, pageNumber, pageSize);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading my tickets");
                TempData["Error"] = "An error occurred while loading your tickets.";
                return View(new PagedResult<TicketListItemViewModel>());
            }
        }

        // GET: Tickets/Assigned
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Assigned(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _ticketService.GetMyAssignedTicketsAsync(userId, pageNumber, pageSize);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assigned tickets");
                TempData["Error"] = "An error occurred while loading assigned tickets.";
                return View(new PagedResult<TicketListItemViewModel>());
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var ticket = await _ticketService.GetTicketDetailsAsync(id.Value);
                if (ticket == null)
                    return NotFound();

                var statuses = await _ticketService.GetStatusesAsync();
                var users = _context.Users.Where(u => u.IsActive).Select(u => new { u.Id, u.UserName }).ToList();

                ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");
                ViewBag.Users = new SelectList(users, "Id", "UserName");

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading ticket details for {id}");
                TempData["Error"] = "An error occurred while loading ticket details.";
                return NotFound();
            }
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _ticketService.GetCategoriesAsync();
                var priorities = await _ticketService.GetPrioritiesAsync();

                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name");

                return View(new TicketCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create ticket form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _ticketService.GetCategoriesAsync();
                var priorities = await _ticketService.GetPrioritiesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
                ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name", model.PriorityId);
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var (success, message, ticketId) = await _ticketService.CreateTicketAsync(model, userId);

                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }

                TempData["Error"] = message;
                var cats = await _ticketService.GetCategoriesAsync();
                var priors = await _ticketService.GetPrioritiesAsync();
                ViewBag.Categories = new SelectList(cats, "CategoryId", "Name", model.CategoryId);
                ViewBag.Priorities = new SelectList(priors, "PriorityId", "Name", model.PriorityId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                TempData["Error"] = "An unexpected error occurred while creating the ticket.";
                return View(model);
            }
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var ticketDetails = await _ticketService.GetTicketDetailsAsync(id.Value);
                if (ticketDetails == null)
                    return NotFound();

                var model = new TicketEditViewModel
                {
                    TicketId = ticketDetails.TicketId,
                    TicketNumber = ticketDetails.TicketNumber,
                    Title = ticketDetails.Title,
                    Description = ticketDetails.Description,
                    CategoryName = ticketDetails.CategoryName,
                    PriorityName = ticketDetails.PriorityName,
                    DueBy = ticketDetails.DueBy,
                    CreatedOn = ticketDetails.CreatedOn,
                    ResolvedOn = ticketDetails.ResolvedOn,
                    ClosedOn = ticketDetails.ClosedOn,
                    RequesterName = ticketDetails.RequesterName
                };

                var categories = await _ticketService.GetCategoriesAsync();
                var priorities = await _ticketService.GetPrioritiesAsync();
                var statuses = await _ticketService.GetStatusesAsync();
                var users = _context.Users.Where(u => u.IsActive).Select(u => new { u.Id, u.UserName }).ToList();

                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
                ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name", model.PriorityId);
                ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name", model.StatusId);
                ViewBag.Users = new SelectList(users, "Id", "UserName", model.AssignedToId);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for ticket {id}");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(Guid id, TicketEditViewModel model)
        {
            if (id != model.TicketId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var (success, message) = await _ticketService.UpdateTicketAsync(model);
                    if (success)
                    {
                        TempData["Success"] = message;
                        return RedirectToAction(nameof(Details), new { id = model.TicketId });
                    }

                    TempData["Error"] = message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating ticket {id}");
                    TempData["Error"] = "An unexpected error occurred.";
                }
            }

            var categories = await _ticketService.GetCategoriesAsync();
            var priorities = await _ticketService.GetPrioritiesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
            ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name", model.PriorityId);

            return View(model);
        }

        // POST: Tickets/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Assign(Guid id, Guid? assignedToId)
        {
            try
            {
                var (success, message) = await _ticketService.AssignTicketAsync(id, assignedToId);
                TempData[success ? "Success" : "Error"] = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning ticket {id}");
                TempData["Error"] = "An unexpected error occurred.";
            }

            // If coming from AssignmentManagement, redirect back there
            if (!string.IsNullOrEmpty(Request.Headers["Referer"]) && Request.Headers["Referer"].ToString().Contains("AssignmentManagement"))
            {
                return RedirectToAction(nameof(AssignmentManagement));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Tickets/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(Guid id, Guid statusId)
        {
            try
            {
                var (success, message) = await _ticketService.UpdateStatusAsync(id, statusId);
                TempData[success ? "Success" : "Error"] = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for ticket {id}");
                TempData["Error"] = "An unexpected error occurred.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Tickets/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            try
            {
                var (success, message) = await _ticketService.ResolveTicketAsync(id);
                TempData[success ? "Success" : "Error"] = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving ticket {id}");
                TempData["Error"] = "An unexpected error occurred.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Tickets/Reopen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Reopen(Guid id)
        {
            try
            {
                var (success, message) = await _ticketService.ReopenTicketAsync(id);
                TempData[success ? "Success" : "Error"] = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reopening ticket {id}");
                TempData["Error"] = "An unexpected error occurred.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var ticketDetails = await _ticketService.GetTicketDetailsAsync(id.Value);
                if (ticketDetails == null)
                    return NotFound();

                return View(ticketDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for ticket {id}");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ticket {id}");
                TempData["Error"] = "An error occurred while deleting the ticket.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || ticketId == Guid.Empty)
            {
                TempData["Error"] = "Invalid comment.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            try
            {
                var userId = GetCurrentUserId();
                var comment = new Comment
                {
                    CommentId = Guid.NewGuid(),
                    TicketId = ticketId,
                    AuthorId = userId,
                    Content = content,
                    CreatedOn = DateTime.UtcNow,
                    IsInternal = false
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Comment added successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding comment to ticket {ticketId}");
                TempData["Error"] = "An error occurred while adding the comment.";
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // POST: Tickets/UploadAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachment(Guid ticketId, IFormFile? file)
        {
            if (file == null || file.Length == 0 || ticketId == Guid.Empty)
            {
                TempData["Error"] = "Invalid file upload.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            try
            {
                // Validate file size (max 10 MB)
                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    TempData["Error"] = "File size exceeds 10 MB limit.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }

                // Validate file types
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".7z" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "File type not allowed.";
                    return RedirectToAction(nameof(Details), new { id = ticketId });
                }

                // Save file to disk (you may want to use cloud storage instead)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var userId = GetCurrentUserId();
                var attachment = new Attachment
                {
                    AttachmentId = Guid.NewGuid(),
                    TicketId = ticketId,
                    FileName = file.FileName,
                    FilePath = $"/uploads/tickets/{uniqueFileName}",
                    FileSize = file.Length,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    UploadedById = userId,
                    UploadedOn = DateTime.UtcNow
                };

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading attachment for ticket {ticketId}");
                TempData["Error"] = "An error occurred while uploading the file.";
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // GET: Tickets/AssignmentManagement
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignmentManagement(string? priority = null, string? category = null, string? sortBy = "created")
        {
            try
            {
                // Get unassigned tickets
                var unassignedTickets = _context.Tickets
                    .Where(t => t.AssignedToId == null)
                    .Include(t => t.Requester)
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrEmpty(priority))
                {
                    unassignedTickets = unassignedTickets.Where(t => t.Priority.Name == priority);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    unassignedTickets = unassignedTickets.Where(t => t.Category.Name == category);
                }

                // Apply sorting
                var sorted = sortBy switch
                {
                    "dueDate" => unassignedTickets.OrderBy(t => t.DueBy),
                    "priority" => unassignedTickets.OrderBy(t => t.Priority.Name),
                    _ => unassignedTickets.OrderByDescending(t => t.CreatedOn)
                };

                var tickets = await sorted.ToListAsync();

                // Calculate statistics
                var allTickets = _context.Tickets.AsNoTracking();
                ViewBag.UnassignedCount = tickets.Count;
                ViewBag.CriticalCount = await allTickets.Where(t => t.AssignedToId == null && t.Priority.Name == "Critical").CountAsync();
                ViewBag.HighCount = await allTickets.Where(t => t.AssignedToId == null && t.Priority.Name == "High").CountAsync();
                ViewBag.OverdueCount = await allTickets.Where(t => t.AssignedToId == null && t.DueBy < DateTime.Now).CountAsync();

                // Load filter options
                var categories = await _ticketService.GetCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");

                // Load available staff (users with Staff or Admin role)
                var staffRoleId = await _context.Roles.Where(r => r.Name == "Staff" || r.Name == "Admin")
                    .Select(r => r.Id).FirstOrDefaultAsync();

                var availableStaff = await _context.UserRoles
                    .Where(ur => ur.RoleId == staffRoleId)
                    .Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => u)
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToListAsync();

                ViewBag.AvailableStaff = new SelectList(availableStaff, "Id", "FullName");
                ViewBag.UnassignedTickets = tickets;

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assignment management");
                TempData["Error"] = "An error occurred while loading assignment data.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
