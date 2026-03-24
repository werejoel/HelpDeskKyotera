using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HelpDeskKyotera.Hubs;

public class LocationHub : Hub
{
    private readonly ILogger<LocationHub> _logger;

    public LocationHub(ILogger<LocationHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Connection {Conn} connected to LocationHub", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Connection {Conn} disconnected from LocationHub", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
