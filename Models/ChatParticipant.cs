using System;

namespace HelpDeskKyotera.Models;

public class ChatParticipant
{
    public Guid ChatParticipantId { get; set; }

    public Guid ChatConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
}
