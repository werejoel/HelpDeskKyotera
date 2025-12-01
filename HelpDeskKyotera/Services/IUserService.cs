using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace HelpDeskKyotera.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserListItemViewModel>> GetUsersAsync(UserListFilterViewModel filter);
        Task<(IdentityResult Result, Guid? UserId)> CreateAsync(UserCreateViewModel model);
        Task<UserEditViewModel?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(UserEditViewModel model);
        Task<UserDetailsViewModel?> GetDetailsAsync(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
        Task<UserRolesEditViewModel?> GetRolesForEditAsync(Guid userId);
        Task<IdentityResult> UpdateRolesAsync(Guid userId, IEnumerable<Guid> selectedRoleIds);

        //New Methods for User Claim Management
        Task<UserClaimsEditViewModel?> GetClaimsForEditAsync(Guid userId);
        Task<IdentityResult> UpdateClaimsAsync(Guid userId, IEnumerable<Guid> selectedClaimIds);
    }
}