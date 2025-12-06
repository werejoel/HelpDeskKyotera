namespace HelpDeskKyotera.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; } = Guid.NewGuid();
        public Guid TicketId { get; set; }
        public Guid AuthorId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsInternal { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public virtual Ticket Ticket { get; set; } = null!;
        public virtual ApplicationUser Author { get; set; } = null!;
    }
}
