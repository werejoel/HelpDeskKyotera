// HelpDeskKyotera/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();

    // CHANGE THESE TO Guid? (NOT string!)
    public Guid? DepartmentId { get; set; }
    public Guid? LocationId { get; set; }

    public string? JobTitle { get; set; }
    public string? EmployeeId { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedOn { get; set; }

    // Navigation
    public virtual Department? Department { get; set; }
    public virtual Location? Location { get; set; }

    public virtual ICollection<Ticket> RequestedTickets { get; set; }
    public virtual ICollection<Ticket> AssignedTickets { get; set; }
    public virtual ICollection<Comment> Comments { get; set; }
}