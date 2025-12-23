using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
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
