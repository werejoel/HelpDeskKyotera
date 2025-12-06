using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace HelpDeskKyotera.Models
{
    public class Priority
    {
        public Guid PriorityId { get; set; } = Guid.NewGuid();
        [Required, StringLength(50)]
        public string Name { get; set; } = null!; // Low, Medium, High, Critical
        public int ResponseSLA { get; set; } // hours
        public int ResolutionSLA { get; set; } // hours
        public int Order { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
