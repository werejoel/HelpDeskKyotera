namespace HelpDeskKyotera.Models
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = null!;
        public Guid UploadedById { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

        public Guid? TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual ApplicationUser UploadedBy { get; set; } = null!;
    }
}
