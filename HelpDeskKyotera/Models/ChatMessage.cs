using System;

namespace HelpDeskKyotera.Models;

public class ChatMessage
{
    public Guid ChatMessageId { get; set; }

    public Guid ChatConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;

    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
}
