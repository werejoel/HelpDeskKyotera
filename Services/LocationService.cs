using System;
using System.Threading.Tasks;
using HelpDeskKyotera.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HelpDeskKyotera.Services;

public class LocationService : ILocationService
{
    private readonly IHubContext<LocationHub> _hub;
    private readonly ISmsSender? _smsSender;
    private readonly ILogger<LocationService> _logger;

    public LocationService(IHubContext<LocationHub> hub, ILogger<LocationService> logger, ISmsSender? smsSender = null)
    {
        _hub = hub;
        _logger = logger;
        _smsSender = smsSender;
    }

    public async Task BroadcastLocationAsync(string userId, double latitude, double longitude, DateTime? timestamp = null, string? phoneToNotify = null, string? smsMessage = null)
    {
        var ts = timestamp ?? DateTime.UtcNow;
        var payload = new
        {
            UserId = userId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = ts
        };

        _logger.LogDebug("Broadcasting location for {UserId}: {Lat},{Lon}", userId, latitude, longitude);
        await _hub.Clients.All.SendAsync("ReceiveLocation", payload);

        if (!string.IsNullOrWhiteSpace(phoneToNotify) && !string.IsNullOrWhiteSpace(smsMessage) && _smsSender != null)
        {
            try
            {
                await _smsSender.SendSmsAsync(phoneToNotify, smsMessage);
                _logger.LogInformation("Sent SMS notification to {Phone} for user {UserId}", phoneToNotify, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS notification to {Phone}", phoneToNotify);
            }
        }
    }
}
