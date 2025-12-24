using HelpDeskKyotera.Models;
namespace HelpDeskKyotera.Services;

public interface IChatService
{
    Task<ChatConversation> GetOrCreateConversationForTicketAsync(Guid ticketId);
    Task<ChatConversation?> GetConversationAsync(Guid conversationId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId, int page = 0, int pageSize = 100);
    Task<ChatMessage> AddMessageAsync(Guid conversationId, Guid senderId, string body);
    Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(Guid userId);
}
