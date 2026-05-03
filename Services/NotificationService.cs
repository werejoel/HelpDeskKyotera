using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;

        public NotificationService(ApplicationDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task CreateNotificationAsync(Guid? userId, string? email, string subject, string body, string? link = null)
        {
            var n = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Email = email,
                Subject = subject,
                Body = body,
                Link = link,
                IsRead = false,
                CreatedOn = DateTime.UtcNow
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
        }

        public async Task CreateNotificationAndSendEmailAsync(Guid? userId, string? email, string subject, string body, string? link = null)
        {
            // Create the notification in database
            await CreateNotificationAsync(userId, email, subject, body, link);

            // Send email if email address is provided
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    await _emailService.SendHtmlEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email notification: {ex.Message}");
                    // Continue even if email fails, notification is already saved
                }
            }
        }

        public async Task<IList<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 10)
        {
            return await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId || (n.UserId == null && n.Email == null))
                .OrderByDescending(n => n.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);
            if (n == null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
