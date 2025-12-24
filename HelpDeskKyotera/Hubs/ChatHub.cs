using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskKyotera.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatHub(IChatService chatService, UserManager<ApplicationUser> userManager)
    {
        _chatService = chatService;
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        // Join all conversation groups for the connected user so they receive messages
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user != null)
        {
            var convs = await _chatService.GetUserConversationsAsync(user.Id);
            foreach (var c in convs)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, c.ChatConversationId.ToString());
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendMessageToConversation(Guid conversationId, string message)
    {
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user == null) return;

        var msg = await _chatService.AddMessageAsync(conversationId, user.Id, message);

        var payload = new
        {
            ConversationId = conversationId,
            MessageId = msg.ChatMessageId,
            SenderId = user.Id,
            SenderName = user.FirstName + " " + user.LastName,
            Body = msg.Body,
            CreatedOn = msg.CreatedOn
        };

        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", payload);
    }
}
