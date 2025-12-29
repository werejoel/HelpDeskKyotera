using System;
using System.Threading.Tasks;

namespace HelpDeskKyotera.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid? userId, string? email, string subject, string body, string? link = null);
        Task<IList<HelpDeskKyotera.Models.Notification>> GetUserNotificationsAsync(Guid userId, int limit = 10);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
    }
}
