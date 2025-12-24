using System.Security.Claims;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
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
}
