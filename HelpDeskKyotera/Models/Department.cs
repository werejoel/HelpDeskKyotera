using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskKyotera.Models
{
    public class Department
    {
        public Guid DepartmentId { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public Guid? HeadOfDepartmentId { get; set; }
        public Guid? LocationId { get; set; }

        // Navigation
        public virtual ApplicationUser? Head { get; set; }
        public virtual Location? Location { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}