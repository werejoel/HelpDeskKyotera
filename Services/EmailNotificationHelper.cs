using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Http;

namespace HelpDeskKyotera.Services
{
    /// <summary>
    /// Helper class to simplify email notification sending from controllers
    /// </summary>
    public class EmailNotificationHelper
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailNotificationHelper(
            IEmailService emailService, 
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _emailService = emailService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Build full URL for ticket link
        /// </summary>
        public string BuildTicketLink(Guid ticketId)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return $"/Tickets/Details/{ticketId}";
            
            return $"{request.Scheme}://{request.Host}/Tickets/Details/{ticketId}";
        }

        /// <summary>
        /// Send new ticket notification to multiple recipients
        /// </summary>
        public async Task SendNewTicketNotificationAsync(
            IEnumerable<ApplicationUser> recipients,
            string ticketNumber,
            string title,
            string requesterName,
            Guid ticketId)
        {
            var ticketLink = BuildTicketLink(ticketId);

            foreach (var recipient in recipients.Where(r => !string.IsNullOrEmpty(r.Email)))
            {
                try
                {
                    await _emailService.SendNewTicketNotificationAsync(
                        toEmail: recipient.Email,
                        recipientName: recipient.UserName,
                        ticketNumber: ticketNumber,
                        title: title,
                        requesterName: requesterName,
                        ticketLink: ticketLink
                    );

                    // Also save to database
                    await _notificationService.CreateNotificationAsync(
                        userId: recipient.Id,
                        email: recipient.Email,
                        subject: $"New Ticket: {ticketNumber}",
                        body: $"New ticket created: <strong>{title}</strong>",
                        link: $"/Tickets/Details/{ticketId}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending notification to {recipient.Email}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Send ticket status changed notification
        /// </summary>
        public async Task SendTicketStatusChangeNotificationAsync(
            ApplicationUser? recipient,
            string ticketNumber,
            string title,
            string newStatus,
            Guid ticketId)
        {
            if (recipient?.Email == null) return;

            try
            {
                var ticketLink = BuildTicketLink(ticketId);

                await _emailService.SendTicketStatusChangedAsync(
                    toEmail: recipient.Email,
                    recipientName: recipient.UserName,
                    ticketNumber: ticketNumber,
                    title: title,
                    newStatus: newStatus,
                    ticketLink: ticketLink
                );

                await _notificationService.CreateNotificationAsync(
                    userId: recipient.Id,
                    email: recipient.Email,
                    subject: $"Ticket {ticketNumber} Status Changed",
                    body: $"Ticket status changed to <strong>{newStatus}</strong>",
                    link: $"/Tickets/Details/{ticketId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status change notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send ticket assignment notification
        /// </summary>
        public async Task SendTicketAssignmentNotificationAsync(
            ApplicationUser? assignedTo,
            string ticketNumber,
            string title,
            string assignedBy,
            Guid ticketId)
        {
            if (assignedTo?.Email == null) return;

            try
            {
                var ticketLink = BuildTicketLink(ticketId);

                await _emailService.SendTicketAssignedAsync(
                    toEmail: assignedTo.Email,
                    recipientName: assignedTo.UserName,
                    ticketNumber: ticketNumber,
                    title: title,
                    assignedBy: assignedBy,
                    ticketLink: ticketLink
                );

                await _notificationService.CreateNotificationAsync(
                    userId: assignedTo.Id,
                    email: assignedTo.Email,
                    subject: $"Ticket {ticketNumber} Assigned to You",
                    body: $"You have been assigned to ticket <strong>{ticketNumber}</strong>",
                    link: $"/Tickets/Details/{ticketId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending assignment notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send comment added notification
        /// </summary>
        public async Task SendCommentNotificationAsync(
            IEnumerable<ApplicationUser> recipients,
            string ticketNumber,
            string commenterName,
            Guid ticketId)
        {
            var ticketLink = BuildTicketLink(ticketId);

            foreach (var recipient in recipients.Where(r => !string.IsNullOrEmpty(r.Email)))
            {
                try
                {
                    await _emailService.SendCommentAddedAsync(
                        toEmail: recipient.Email,
                        recipientName: recipient.UserName,
                        ticketNumber: ticketNumber,
                        commenterName: commenterName,
                        ticketLink: ticketLink
                    );

                    await _notificationService.CreateNotificationAsync(
                        userId: recipient.Id,
                        email: recipient.Email,
                        subject: $"New Comment on Ticket {ticketNumber}",
                        body: $"<strong>{commenterName}</strong> added a comment",
                        link: $"/Tickets/Details/{ticketId}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending comment notification: {ex.Message}");
                }
            }
        }
    }
}
