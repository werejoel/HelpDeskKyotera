using System.Security.Claims;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    // If ticketId is provided, open or create conversation for that ticket.
    public async Task<IActionResult> Index(Guid? ticketId, Guid? conversationId)
    {
        if (ticketId.HasValue)
        {
            var conv = await _chatService.GetOrCreateConversationForTicketAsync(ticketId.Value);
            return View("Index", conv);
        }

        if (conversationId.HasValue)
        {
            var conv = await _chatService.GetConversationAsync(conversationId.Value);
            if (conv == null) return NotFound();
            return View("Index", conv);
        }

        // list user's conversations
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var convs = await _chatService.GetUserConversationsAsync(userId);
            return View("List", convs);
        }

        return Forbid();
    }
}
