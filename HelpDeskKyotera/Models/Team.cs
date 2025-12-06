using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace HelpDeskKyotera.Models
{
    public class Team
    {
        public Guid TeamId { get; set; } = Guid.NewGuid();
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Specialization { get; set; }
        public Guid? TeamLeadId { get; set; }

        public virtual ApplicationUser? TeamLead { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
