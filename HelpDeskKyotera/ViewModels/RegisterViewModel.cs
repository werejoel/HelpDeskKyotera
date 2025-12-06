using System.ComponentModel.DataAnnotations;
using HelpDeskKyotera.ViewModels.Users;

namespace HelpDeskKyotera.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email Id is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "PhoneNumber is Required")]
        [Phone(ErrorMessage = "Please enter a valid Phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Select Roles")]
        public IEnumerable<Guid>? SelectedRoleIds { get; set; } = new List<Guid>();

        // For UI display purposes
        public IEnumerable<RoleCheckboxItem> AvailableRoles { get; set; } = new List<RoleCheckboxItem>();
    }
}
