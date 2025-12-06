using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace HelpDeskKyotera.Models
{
    public class Status
    {
        public Guid StatusId { get; set; } = Guid.NewGuid();
        [Required, StringLength(50)]
        public string Name { get; set; } = null!;
        public bool IsFinal { get; set; }
        public int Order { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}