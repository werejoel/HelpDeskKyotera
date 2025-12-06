using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace HelpDeskKyotera.Models
{
    public class Category
    {
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? DefaultTeamId { get; set; }

        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
        public virtual Team? DefaultTeam { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
