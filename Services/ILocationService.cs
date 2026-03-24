using System.Threading.Tasks;

namespace HelpDeskKyotera.Services;

public interface ILocationService
{
    Task BroadcastLocationAsync(string userId, double latitude, double longitude, DateTime? timestamp = null, string? phoneToNotify = null, string? smsMessage = null);
}
