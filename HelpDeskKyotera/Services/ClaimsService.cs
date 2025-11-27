using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Claims;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services
{
    // Application service for Claims catalog (ClaimMasters).
    // Encapsulates query logic (search, filter, paging) and CRUD with basic guards.
    // Controllers should call this service instead of touching DbContext directly.
    public class ClaimsService : IClaimsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClaimsService> _logger;

        // Hard guard for paging; avoid accidental "SELECT *" with huge pageSize.
        private const int MaxPageSize = 100;

        public ClaimsService(ApplicationDbContext context, ILogger<ClaimsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Returns a paged, filtered list of claims for the Index grid.
        // - search: matches ClaimType, ClaimValue, or Description (contains).
        // - category: exact match on Category ("User", "Role", "Both").
        // Read path is AsNoTracking for best performance.
        public async Task<PagedResult<ClaimListItemViewModel>> GetPagedClaimsAsync(
            string? search, string? category, int pageNumber, int pageSize)
        {
            try
            {
                // Defensive paging (never page 0 or negative; clamp size).
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, MaxPageSize);

                // Read-only query → AsNoTracking avoids change-tracker overhead.
                var query = _context.ClaimMasters.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    // NOTE:
                    // Contains translates to SQL LIKE '%search%'.
                    // This is user-friendly for admins and fine for moderate datasets.
                    // For very large tables, consider full-text search.
                    query = query.Where(c =>
                        c.ClaimType.Contains(search) ||
                        c.ClaimValue.Contains(search) ||
                        (c.Description != null && c.Description.Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    category = category.Trim();
                    query = query.Where(c => c.Category == category);
                }

                // Count first (before Skip/Take); EF will translate efficiently.
                var totalCount = await query.CountAsync();

                // Stable ordering ensures consistent paging between requests.
                var claims = await query
                    .OrderBy(c => c.ClaimType)
                    .ThenBy(c => c.ClaimValue)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ClaimListItemViewModel
                    {
                        Id = c.Id,
                        ClaimType = c.ClaimType,
                        ClaimValue = c.ClaimValue,
                        Category = c.Category,
                        Description = c.Description,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                return new PagedResult<ClaimListItemViewModel>
                {
                    Items = claims,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paged claims (search: {Search}, category: {Category}, page: {Page}, size: {Size})",
                    search, category, pageNumber, pageSize);

                // Fail soft: return empty page but preserve requested paging info.
                return new PagedResult<ClaimListItemViewModel>
                {
                    Items = Array.Empty<ClaimListItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        // Returns a single claim mapped to the edit view model.
        // Read path uses AsNoTracking since the entity is reloaded in Update anyway.
        public async Task<ClaimEditViewModel?> GetClaimByIdAsync(Guid id)
        {
            try
            {
                return await _context.ClaimMasters
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => new ClaimEditViewModel
                    {
                        Id = c.Id,
                        ClaimType = c.ClaimType,
                        ClaimValue = c.ClaimValue,
                        Category = c.Category,
                        Description = c.Description,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim by Id {Id}", id);
                return null;
            }
        }

        // Creates a new claim in the catalog after performing duplicate checks.
        // Returns (Success, Message) suitable for TempData or model-level errors.
        public async Task<(bool Success, string Message)> CreateClaimAsync(ClaimEditViewModel model)
        {
            try
            {
                // Basic field validation.
                if (string.IsNullOrWhiteSpace(model.ClaimType) || string.IsNullOrWhiteSpace(model.ClaimValue))
                    return (false, "Claim Type and Claim Value are required.");

                // Normalize input to reduce duplicate risk from accidental whitespace.
                var type = model.ClaimType.Trim();
                var value = model.ClaimValue.Trim();
                var category = model.Category?.Trim();

                // Duplicate check: same Type + Value + Category not allowed.
                bool exists = await _context.ClaimMasters
                    .AnyAsync(c => c.ClaimType == type &&
                                   c.ClaimValue == value &&
                                   c.Category == category);

                if (exists)
                    return (false, "A claim with the same type, value, and category already exists.");

                var claim = new ClaimMaster
                {
                    Id = Guid.NewGuid(),
                    ClaimType = type,
                    ClaimValue = value,
                    Category = category!,
                    Description = model.Description?.Trim(),
                    IsActive = model.IsActive,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                _context.ClaimMasters.Add(claim);
                await _context.SaveChangesAsync();

                return (true, "Claim created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim (Type: {Type}, Value: {Value}, Category: {Category})",
                    model.ClaimType, model.ClaimValue, model.Category);
                return (false, "An error occurred while creating the claim.");
            }
        }

        // Updates an existing claim after duplicate checks.
        // Uses FindAsync to load the tracked entity, then mutates properties and saves.
        public async Task<(bool Success, string Message)> UpdateClaimAsync(ClaimEditViewModel model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                    return (false, "Invalid claim identifier.");

                var claim = await _context.ClaimMasters.FindAsync(model.Id);
                if (claim == null)
                    return (false, "Claim not found.");

                if (string.IsNullOrWhiteSpace(model.ClaimType) || string.IsNullOrWhiteSpace(model.ClaimValue))
                    return (false, "Claim Type and Claim Value are required.");

                var type = model.ClaimType.Trim();
                var value = model.ClaimValue.Trim();
                var category = model.Category?.Trim();

                // Duplicate check excluding self.
                bool duplicateExists = await _context.ClaimMasters
                    .AnyAsync(c => c.Id != model.Id &&
                                   c.ClaimType == type &&
                                   c.ClaimValue == value &&
                                   c.Category == category);

                if (duplicateExists)
                    return (false, "Another claim with the same type, value, and category already exists.");

                // Apply updates to the tracked entity.
                claim.ClaimType = type;
                claim.ClaimValue = value;
                claim.Category = category!;
                claim.Description = model.Description?.Trim();
                claim.IsActive = model.IsActive;
                claim.ModifiedOn = DateTime.UtcNow;

                // No need to call _context.Update(claim) here since it's tracked; SaveChanges will detect changes.
                await _context.SaveChangesAsync();

                return (true, "Claim updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating claim Id {Id}", model.Id);
                return (false, "An error occurred while updating the claim.");
            }
        }

        // Soft delete: marks IsActive = false.
        // Keeps history and prevents broken references.
        // Returns true if the record existed and was deactivated.
        public async Task<bool> DeleteClaimAsync(Guid id)
        {
            try
            {
                var claim = await _context.ClaimMasters.FindAsync(id);
                if (claim == null) return false;

                if (!claim.IsActive)
                {
                    // Already inactive; treat as success to keep the endpoint idempotent.
                    return true;
                }

                claim.IsActive = false;
                claim.ModifiedOn = DateTime.UtcNow;

                // Tracked entity → SaveChanges is enough.
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting (deactivating) claim Id {Id}", id);
                return false;
            }
        }
    }
}