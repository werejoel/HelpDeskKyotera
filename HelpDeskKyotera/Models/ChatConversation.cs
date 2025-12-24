using System;
using System.Collections.Generic;

namespace HelpDeskKyotera.Models;

public class ChatConversation
{
    public Guid ChatConversationId { get; set; }

    // Optional link to a Ticket
    public Guid? TicketId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
}
