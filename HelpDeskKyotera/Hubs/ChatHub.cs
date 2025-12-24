using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskKyotera.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, UserManager<ApplicationUser> userManager, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        // Join all conversation groups for the connected user so they receive messages
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user != null)
        {
            _logger.LogInformation("User {UserId} connected to ChatHub", user.Id);
            var convs = await _chatService.GetUserConversationsAsync(user.Id);
            foreach (var c in convs)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, c.ChatConversationId.ToString());
                _logger.LogDebug("Added connection {Conn} to group {Group}", Context.ConnectionId, c.ChatConversationId);
            }
        }
        else
        {
            _logger.LogInformation("Anonymous or invalid user connected to ChatHub: {Conn}", Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(Guid conversationId)
    {
        _logger.LogDebug("Connection {Conn} joining conversation {Conv}", Context.ConnectionId, conversationId);
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendMessageToConversation(Guid conversationId, string message)
    {
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user == null) return;

        _logger.LogInformation("User {UserId} sending message to {Conv}", user.Id, conversationId);
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
        _logger.LogDebug("Broadcasted message {MsgId} to conversation {Conv}", msg.ChatMessageId, conversationId);
    }
}
