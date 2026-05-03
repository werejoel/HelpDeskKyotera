namespace HelpDeskKyotera.Services
{

    public interface IEmailService
    {
        Task SendRegistrationConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink);
        Task SendAccountCreatedEmailAsync(string toEmail, string firstName, string loginLink);
        Task SendResendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink);
        Task SendHtmlEmailAsync(string toEmail, string subject, string htmlContent);
        Task SendNewTicketNotificationAsync(string toEmail, string recipientName, string ticketNumber, string title, string requesterName, string ticketLink);
        Task SendTicketStatusChangedAsync(string toEmail, string recipientName, string ticketNumber, string title, string newStatus, string ticketLink);
        Task SendTicketAssignedAsync(string toEmail, string recipientName, string ticketNumber, string title, string assignedBy, string ticketLink);
        Task SendCommentAddedAsync(string toEmail, string recipientName, string ticketNumber, string commenterName, string ticketLink);
    }
}