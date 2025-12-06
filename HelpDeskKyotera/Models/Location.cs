using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.Models
{
    public class Location
    {
        public Guid LocationId { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Room { get; set; }
        public string? City { get; set; } = "Kyotera";
        public string? Country { get; set; } = "Uganda";

        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
