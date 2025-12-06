using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace HelpDeskKyotera.Models
{
    public class Ticket
    {
        public Guid TicketId { get; set; } = Guid.NewGuid();
        public string TicketNumber { get; set; } = null!; // e.g., INC20250001
        [Required, StringLength(200)]
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;



        public Guid CategoryId { get; set; }
        public Guid PriorityId { get; set; }
        public Guid StatusId { get; set; }
        public Guid RequesterId { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid? TeamId { get; set; }


        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? DueBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime? ClosedOn { get; set; }



        // Navigation
        public virtual ApplicationUser Requester { get; set; } = null!;
        public virtual ApplicationUser? AssignedTo { get; set; }
        public virtual Team? Team { get; set; }
        public virtual Category Category { get; set; } = null!;
        public virtual Priority Priority { get; set; } = null!;
        public virtual Status Status { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
