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
            .Include(c => c.Messages.OrderBy(m => m.CreatedOn))
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId);
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
        return await _db.ChatParticipants
            .Where(p => p.UserId == userId)
            .Select(p => p.Conversation)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedOn).Take(1))
            .ToListAsync();
    }
}
