using System;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskKyotera.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        // Optional target user (if null, notification is generic / email-only)
        public Guid? UserId { get; set; }

        // Optional email address the notification was sent to
        public string? Email { get; set; }

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!;

        public string? Link { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
