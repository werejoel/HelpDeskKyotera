using System.Security.Claims;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HelpDeskKyotera.Hubs;

namespace HelpDeskKyotera.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages([FromQuery] Guid conversationId, int page = 0, int pageSize = 100)
    {
        var msgs = await _chatService.GetMessagesAsync(conversationId, page, pageSize);
        var shaped = msgs.Select(m => new {
            m.ChatMessageId,
            m.ChatConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FirstName + " " + m.Sender?.LastName,
            m.Body,
            m.CreatedOn
        });
        return Ok(shaped);
    }

    public class PostMessageDto
    {
        public Guid ConversationId { get; set; }
        public string? Body { get; set; }
    }

    [HttpPost("messages")]
    public async Task<IActionResult> PostMessage([FromBody] PostMessageDto dto)
    {
        if (dto == null || dto.Body == null) return BadRequest();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return Forbid();

        var msg = await _chatService.AddMessageAsync(dto.ConversationId, userId, dto.Body);

        var payload = new
        {
            ConversationId = dto.ConversationId,
            MessageId = msg.ChatMessageId,
            SenderId = userId,
            SenderName = msg.Sender?.FirstName + " " + msg.Sender?.LastName,
            Body = msg.Body,
            CreatedOn = msg.CreatedOn
        };

        try
        {
            await _hubContext.Clients.Group(dto.ConversationId.ToString()).SendAsync("ReceiveMessage", payload);
        }
        catch { }

        return Ok(payload);
    }
}
