using HelpDeskKyotera.ViewModels.Users;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.ViewModels.Users
{
    public class UserClaimsEditViewModel
    {
        [Required(ErrorMessage = "Invalid user.")]
        public Guid UserId { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        // Only claims from Category = User or Both (and IsActive)
        public List<UserClaimCheckboxItem> Claims { get; set; } = new();
    }
}