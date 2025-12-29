using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.ViewModels.Tickets
{
    public class TicketCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [Display(Name = "Priority")]
        public Guid PriorityId { get; set; }

        [Display(Name = "Due By")]
        public DateTime? DueBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Title should not be only whitespace
            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult("Title is required.", new[] { nameof(Title) });
            }

            // Description should not be empty
            if (string.IsNullOrWhiteSpace(Description))
            {
                yield return new ValidationResult("Description is required.", new[] { nameof(Description) });
            }

            // Due date should not be in the past
            if (DueBy.HasValue && DueBy.Value.Date < DateTime.UtcNow.Date)
            {
                yield return new ValidationResult("Due date cannot be in the past.", new[] { nameof(DueBy) });
            }
        }
    }

    public class TicketEditViewModel : IValidatableObject
    {
        [Required]
        public Guid TicketId { get; set; }

        [Display(Name = "Ticket Number")]
        public string? TicketNumber { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [Display(Name = "Priority")]
        public Guid PriorityId { get; set; }

        [Display(Name = "Status")]
        public Guid StatusId { get; set; }

        [Display(Name = "Assigned To")]
        public Guid? AssignedToId { get; set; }

        [Display(Name = "Due By")]
        public DateTime? DueBy { get; set; }

        public string? CategoryName { get; set; }
        public string? PriorityName { get; set; }
        public string? RequesterName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime? ClosedOn { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Title))
                yield return new ValidationResult("Title is required.", new[] { nameof(Title) });

            if (string.IsNullOrWhiteSpace(Description))
                yield return new ValidationResult("Description is required.", new[] { nameof(Description) });

            if (DueBy.HasValue && DueBy.Value.Date < DateTime.UtcNow.Date)
                yield return new ValidationResult("Due date cannot be in the past.", new[] { nameof(DueBy) });
        }
    }

    public class TicketListItemViewModel
    {
        public Guid TicketId { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string PriorityName { get; set; } = null!;
        public string StatusName { get; set; } = null!;
        public string RequesterName { get; set; } = null!;
        public string? AssignedToName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueBy { get; set; }
        public bool IsOverdue => DueBy.HasValue && DueBy.Value < DateTime.UtcNow && StatusName != "Closed" && StatusName != "Resolved";
    }

    public class TicketDetailsViewModel
    {
        public Guid RequesterId { get; set; }
        public Guid TicketId { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string PriorityName { get; set; } = null!;
        public string StatusName { get; set; } = null!;
        public Guid StatusId { get; set; }
        public string RequesterName { get; set; } = null!;
        public string? AssignedToName { get; set; }
        public Guid? AssignedToId { get; set; }
        public string? TeamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
        public bool IsOverdue => DueBy.HasValue && DueBy.Value < DateTime.UtcNow && StatusName != "Closed" && StatusName != "Resolved";
    }

    public class TicketFilterViewModel
    {
        public string? Search { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? PriorityId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AssignedToId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class StatusDropdownViewModel
    {
        public Guid StatusId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class PriorityDropdownViewModel
    {
        public Guid PriorityId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class CategoryDropdownViewModel
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class AssignTicketViewModel
    {
        [Required]
        public Guid TicketId { get; set; }

        [Display(Name = "Assign To User")]
        public Guid? AssignedToId { get; set; }
    }

    public class UpdateStatusViewModel
    {
        [Required]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
        public Guid StatusId { get; set; }
    }
}
