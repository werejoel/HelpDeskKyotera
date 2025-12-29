using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskKyotera.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;

    public ChatService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ChatConversation> GetOrCreateConversationForTicketAsync(Guid ticketId)
    {
        var conv = await _db.ChatConversations
            .Include(c => c.Messages)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.TicketId == ticketId);

        if (conv != null) return conv;

        conv = new ChatConversation { ChatConversationId = Guid.NewGuid(), TicketId = ticketId };
        _db.ChatConversations.Add(conv);
        await _db.SaveChangesAsync();
        return conv;
    }

    public async Task<ChatConversation?> GetConversationAsync(Guid conversationId)
    {
        return await _db.ChatConversations
            .Where(c => c.ChatConversationId == conversationId)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId, int page = 0, int pageSize = 100)
    {
        return await _db.ChatMessages
            .Where(m => m.ChatConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedOn)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ChatMessage> AddMessageAsync(Guid conversationId, Guid senderId, string body)
    {
        var msg = new ChatMessage
        {
            ChatMessageId = Guid.NewGuid(),
            ChatConversationId = conversationId,
            SenderId = senderId,
            Body = body,
            CreatedOn = DateTime.UtcNow
        };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();
        // load sender navigation for immediate use
        await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();
        return msg;
    }

    public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(Guid userId)
    {
        // Get conversations where the user is a participant or is the ticket requester
        return await _db.ChatConversations
            .Where(c => c.Participants.Any(p => p.UserId == userId) ||
                        (c.TicketId != null && c.Ticket!.RequesterId == userId) ||
                        (c.TicketId != null && c.Ticket!.AssignedToId == userId))
            .Include(c => c.Participants)
            .Include(c => c.Ticket)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
            .ToListAsync();
    }
}
