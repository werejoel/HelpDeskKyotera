using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskKyotera.Services
{
  
    public class UserService : IUserService
    {
        private const int MaxPageSize = 100;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }
        public async Task<PagedResult<UserListItemViewModel>> GetUsersAsync(UserListFilterViewModel filter)
        {
            // Normalize and clamp paging inputs to safe values
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > MaxPageSize ? MaxPageSize : filter.PageSize);
            // Base query (read-only fast path)
            var query = _userManager.Users.AsNoTracking();
            // Search heuristic:
            // - If it looks like an email, use NormalizedEmail (indexed).
            // - If it's numeric, filter by PhoneNumber prefix (common usage).
            // - Else, use NormalizedUserName (indexed) + First/Last name prefix.
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                var seachUpper = search.ToUpperInvariant();
                if (search.Contains('@'))
                {
                    query = query.Where(u => u.NormalizedEmail!.StartsWith(seachUpper));
                }
                else if (search.All(char.IsDigit))
                {
                    query = query.Where(u => (u.PhoneNumber ?? "").StartsWith(search));
                }
                else
                {
                    query = query.Where(u =>
                    (u.NormalizedUserName!.StartsWith(seachUpper))
                    || (u.FirstName ?? "").StartsWith(search)
                    || (u.LastName ?? "").StartsWith(search));
                }
            }
            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            if (filter.EmailConfirmed.HasValue)
                query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);
            // Total count for pager (single scalar query)
            var total = await query.CountAsync();
            var items = await query
            // Current sort: friendly alphabetical.
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            // Project only what you need
            .Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email!,
                UserName = u.UserName!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                CreatedOn = u.CreatedOn
            })
            .ToListAsync();
            return new PagedResult<UserListItemViewModel>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        // Creates a new user with password.
        // We rely on Identity's built-in uniqueness/validation (avoid extra pre-check round trip).
        public async Task<(IdentityResult Result, Guid? UserId)> CreateAsync(UserCreateViewModel model)
        {
            // ExecutionStrategy adds resiliency (automatic retries for transient SQL errors)
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<(IdentityResult, Guid?)>(async () =>
            {
                // Start an explicit transaction
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Prepare a new ApplicationUser (keep UserName = Email for simplicity/consistency)
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName?.Trim(),
                        Email = model.Email.Trim(),
                        UserName = model.Email.Trim(),
                        PhoneNumber = model.PhoneNumber,
                        IsActive = model.IsActive,
                        EmailConfirmed = model.MarkEmailConfirmed,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedOn = DateTime.UtcNow
                    };
                    // Let Identity enforce password policy + unique constraints (inside the transaction)
                    var create = await _userManager.CreateAsync(user, model.Password);
                    if (!create.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return (create, null);
                    }
                    await transaction.CommitAsync();
                    return (IdentityResult.Success, user.Id);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // let middleware/logging handle it; caller gets a 500
                }
            });
        }
        // Loads user data for the Edit form (read-only).
        public async Task<UserEditViewModel?> GetForEditAsync(Guid id)
        {
            // AsNoTracking -> we don't need change tracking for display
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return null;
            var vm = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                ConcurrencyStamp = user.ConcurrencyStamp
            };

            // Populate departments for selection
            var departments = await _dbContext.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                .ToListAsync();
            vm.Departments = departments;
            vm.DepartmentId = user.DepartmentId;
            return vm;
        }
        // Updates a user with optimistic concurrency check via ConcurrencyStamp.
        public async Task<IdentityResult> UpdateAsync(UserEditViewModel model)
        {
            // ExecutionStrategy adds resiliency (automatic retries for transient SQL errors)
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(model.Id.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }
                    // Optimistic concurrency guard:
                    // If stamp changed, someone else updated the record
                    if (!string.Equals(user.ConcurrencyStamp, model.ConcurrencyStamp, StringComparison.Ordinal))
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = "ConcurrencyFailure",
                            Description = "This user was modified by another admin. Please reload and try again."
                        });
                    }
                    // If email changed, update both Email & UserName (Identity will SaveChanges inside the transaction)
                    if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
                        if (!emailResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return emailResult;
                        }
                        var usernameResult = await _userManager.SetUserNameAsync(user, model.Email.Trim());
                        if (!usernameResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return usernameResult;
                        }
                    }
                    // Update profile fields
                    user.FirstName = model.FirstName.Trim();
                    user.LastName = model.LastName?.Trim();
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsActive = model.IsActive;
                    user.EmailConfirmed = model.EmailConfirmed;
                    // Update department assignment
                    user.DepartmentId = model.DepartmentId;
                    user.ModifiedOn = DateTime.UtcNow;
                    var update = await _userManager.UpdateAsync(user);
                    if (!update.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return update;
                    }
                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        // Returns detailed view model including assigned roles.
        public async Task<UserDetailsViewModel?> GetDetailsAsync(Guid id)
        {
            // Read-only entity for display
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return null;

            // Identity API requires the user entity for role lookup
            var roles = await _userManager.GetRolesAsync(user);

            var claims = await _userManager.GetClaimsAsync(user);
            var claimTexts = claims
                .OrderBy(c => c.Type).ThenBy(c => c.Value)
                .Select(c => $"{c.Type}: {c.Value}")
                .ToList();

            return new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedOn = user.CreatedOn,
                ModifiedOn = user.ModifiedOn,
                Roles = roles.OrderBy(r => r).ToList(),
                Claims = claimTexts
            };
        }

        // Allows an admin to mark an account's email as confirmed/unconfirmed
        public async Task<IdentityResult> SetEmailConfirmedAsync(Guid userId, bool confirmed)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }

                    user.EmailConfirmed = confirmed;
                    user.ModifiedOn = DateTime.UtcNow;

                    var update = await _userManager.UpdateAsync(user);
                    if (!update.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return update;
                    }

                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        // Deletes a user with a guard to prevent removing the last Admin.
        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }
                    // Safety: block deleting the last "Admin"
                    var adminRole = await _roleManager.FindByNameAsync("Admin");
                    if (adminRole != null)
                    {
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (isAdmin)
                        {
                            var anotherAdminExists = await _dbContext.Set<IdentityUserRole<Guid>>()
                            .AnyAsync(ur => ur.RoleId == adminRole.Id && ur.UserId != user.Id);
                            if (!anotherAdminExists)
                            {
                                await transaction.RollbackAsync();
                                return IdentityResult.Failed(new IdentityError
                                {
                                    Code = "LastAdmin",
                                    Description = "You cannot delete the last user in the 'Admin' role."
                                });
                            }
                        }
                    }
                    var delete = await _userManager.DeleteAsync(user);
                    if (!delete.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return delete;
                    }
                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        // Builds the roles editor (checkbox list) with pre-checked assignments.
        public async Task<UserRolesEditViewModel?> GetRolesForEditAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return null;
            // List all active roles (read-only)
            var allRoles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Where(r => r.IsActive)
            .ToListAsync();
            // Current assignments for the user
            var assignedRoles = await _userManager.GetRolesAsync(user);
            // Case-insensitive check to avoid surprises with different normalizations
            var userRolesEditViewModel = new UserRolesEditViewModel
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Roles = allRoles.Select(role => new RoleCheckboxItem
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    Description = role.Description,
                    IsSelected = assignedRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase)
                }).ToList()
            };
            return userRolesEditViewModel;
        }
        // Updates a user's roles using batched operations
        public async Task<IdentityResult> UpdateRolesAsync(Guid userId, IEnumerable<Guid> selectedRoleIds)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }
                    // Normalize and de-duplicate incoming IDs
                    var ids = (selectedRoleIds ?? Enumerable.Empty<Guid>()).Distinct().ToList();
                    // Map ONLY requested IDs -> names (read-only)
                    var selectedRoleNames = (ids.Count == 0)
                    ? new List<string>()
                    : await _roleManager.Roles
                    .AsNoTracking()
                    .Where(r => ids.Contains(r.Id))
                    .Select(r => r.Name!)
                    .ToListAsync();
                    // Validate existence
                    if (selectedRoleNames.Count != ids.Count)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = "RoleNotFound",
                            Description = "One or more selected roles do not exist."
                        });
                    }
                    // Current roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    // Compute diffs (case-insensitive)
                    var current = new HashSet<string>(currentRoles, StringComparer.OrdinalIgnoreCase);
                    var target = new HashSet<string>(selectedRoleNames, StringComparer.OrdinalIgnoreCase);
                    //current: Admin Manager User
                    //target: Admin Manager CustomerSupport Vendor
                    var toAdd = target.Except(current, StringComparer.OrdinalIgnoreCase).ToList();
                    //toAdd = CustomerSupport Vendor
                    var toRemove = current.Except(target, StringComparer.OrdinalIgnoreCase).ToList();
                    //toRemove = User
                    if (toAdd.Count() == 0 && toRemove.Count() == 0)
                    {
                        await transaction.CommitAsync(); // nothing to do
                        return IdentityResult.Success;
                    }
                    // Batch add/remove to minimize round-trips; both inside the same transaction
                    if (toAdd.Count() > 0)
                    {
                        var add = await _userManager.AddToRolesAsync(user, toAdd);
                        if (!add.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return add;
                        }
                    }
                    if (toRemove.Count() > 0)
                    {
                        var rem = await _userManager.RemoveFromRolesAsync(user, toRemove);
                        if (!rem.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return rem;
                        }
                    }
                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<UserClaimsEditViewModel?> GetClaimsForEditAsync(Guid userId)
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            // Get all active claims that can be assigned to Users or Both
            var allClaims = await _dbContext.ClaimMasters
                .AsNoTracking()
                .Where(c => c.IsActive && (c.Category == "User" || c.Category == "Both"))
                .OrderBy(c => c.ClaimType).ThenBy(c => c.ClaimValue)
                .ToListAsync();

            // Read current user claims from Identity
            var currentClaims = await _userManager.GetClaimsAsync(user);

            var vm = new UserClaimsEditViewModel
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Claims = allClaims.Select(c => new UserClaimCheckboxItem
                {
                    ClaimId = c.Id,
                    ClaimType = c.ClaimType,
                    ClaimValue = c.ClaimValue,
                    Category = c.Category,
                    Description = c.Description,
                    IsSelected = currentClaims.Any(uc => uc.Type == c.ClaimType && uc.Value == c.ClaimValue)
                }).ToList()
            };

            return vm;
        }

        public async Task<IdentityResult> UpdateClaimsAsync(Guid userId, IEnumerable<Guid> selectedClaimIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found." });

            // Only allow choosing from active ClaimMasters in Category = User or Both
            var allowedClaims = await _dbContext.ClaimMasters
                .Where(c => c.IsActive && (c.Category == "User" || c.Category == "Both"))
                .ToListAsync();

            //Selected Claims
            var selected = allowedClaims.Where(c => selectedClaimIds.Contains(c.Id)).ToList();

            //Current Claims
            var currentClaims = await _userManager.GetClaimsAsync(user);

            // Remove old
            foreach (var claim in currentClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            // Add selected
            foreach (var claim in selected)
            {
                await _userManager.AddClaimAsync(user, new Claim(claim.ClaimType, claim.ClaimValue));
            }

            return IdentityResult.Success;
        }

    }
}