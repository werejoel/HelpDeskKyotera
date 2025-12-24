using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.ViewModels.Users
{
    public class UserEditViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Invalid user.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Email Confirmed?")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Department")]
        public Guid? DepartmentId { get; set; }

        // Populated for the edit form
        public IEnumerable<SelectListItem>? Departments { get; set; }

        // Keep this posted via hidden input; adding [HiddenInput] is optional since your view already posts it.
        [Required(ErrorMessage = "Concurrency token is missing. Please reload and try again.")]
        [HiddenInput(DisplayValue = false)]
        public string? ConcurrencyStamp { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure FirstName isn't only whitespace
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                yield return new ValidationResult("First name is required.", new[] { nameof(FirstName) });
            }

            // Email should not contain spaces
            if (!string.IsNullOrEmpty(Email) && Email.Contains(' '))
            {
                yield return new ValidationResult("Email cannot contain spaces.", new[] { nameof(Email) });
            }

            // ConcurrencyStamp must be present
            if (string.IsNullOrWhiteSpace(ConcurrencyStamp))
            {
                yield return new ValidationResult("Concurrency token is missing. Please reload and try again.", new[] { nameof(ConcurrencyStamp) });
            }
        }

    }
}
