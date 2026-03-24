using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] LocationUpdateDto dto)
    {
        if (dto == null) return BadRequest();
        _logger.LogDebug("Received location update for {UserId}", dto.UserId);

        await _locationService.BroadcastLocationAsync(dto.UserId ?? string.Empty, dto.Latitude, dto.Longitude, dto.Timestamp, dto.NotifyPhone, dto.NotifyMessage);
        return Ok(new { status = "ok" });
    }
}

public class LocationUpdateDto
{
    public string? UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime? Timestamp { get; set; }
    // Optional: phone number to notify and custom message
    public string? NotifyPhone { get; set; }
    public string? NotifyMessage { get; set; }
}
