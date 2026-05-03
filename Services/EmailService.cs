using HelpDeskKyotera.Services;
using System.Net;
using System.Net.Mail;

namespace HelpDeskKyotera.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendRegistrationConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#333;'>Welcome, {firstName}!</h2>
                    <p style='font-size:16px; color:#555;'>Thank you for registering. Please confirm your email by clicking the button below.</p>
                    <p style='text-align:center;'>
                      <a href='{confirmationLink}' style='background:#0d6efd; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>Confirm Your Email</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} Dot Net Tutorials. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, "Email Confirmation - Dot Net Tutorials", htmlContent, true);
        }

        public async Task SendAccountCreatedEmailAsync(string toEmail, string firstName, string loginLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#333;'>Hello, {firstName}!</h2>
                    <p style='font-size:16px; color:#555;'>Your account has been successfully created and your email is confirmed.</p>
                    <p style='text-align:center;'>
                      <a href='{loginLink}' style='background:#198754; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>Login to Your Account</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} Dot Net Tutorials. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, "Account Created - Dot Net Tutorials", htmlContent, true);
        }

        public async Task SendResendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#333;'>Hello, {firstName}!</h2>
                    <p style='font-size:16px; color:#555;'>You requested a new email confirmation link. Please confirm your email by clicking the button below.</p>
                    <p style='text-align:center;'>
                      <a href='{confirmationLink}' style='background:#0d6efd; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>Confirm Your Email</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} Dot Net Tutorials. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, "Email Confirmation - Dot Net Tutorials", htmlContent, true);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var password = _configuration["EmailSettings:Password"];

                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };
                message.To.Add(new MailAddress(toEmail));

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

            public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlContent)
            {
              await SendEmailAsync(toEmail, subject, htmlContent, true);
            }

        public async Task SendNewTicketNotificationAsync(string toEmail, string recipientName, string ticketNumber, string title, string requesterName, string ticketLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#0d6efd;'>New Ticket Created</h2>
                    <p style='font-size:16px; color:#555;'>Hello {recipientName},</p>
                    <p style='font-size:16px; color:#555;'>A new ticket has been created and may require your attention.</p>
                    <div style='background-color:#f8f9fa; padding:20px; border-left:4px solid #0d6efd; margin:20px 0;'>
                      <p><strong>Ticket Number:</strong> {ticketNumber}</p>
                      <p><strong>Title:</strong> {title}</p>
                      <p><strong>Requester:</strong> {requesterName}</p>
                    </div>
                    <p style='text-align:center;'>
                      <a href='{ticketLink}' style='background:#0d6efd; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>View Ticket</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} HelpDesk Kyotera. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, $"New Ticket: {ticketNumber} - {title}", htmlContent, true);
        }

        public async Task SendTicketStatusChangedAsync(string toEmail, string recipientName, string ticketNumber, string title, string newStatus, string ticketLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#17a2b8;'>Ticket Status Updated</h2>
                    <p style='font-size:16px; color:#555;'>Hello {recipientName},</p>
                    <p style='font-size:16px; color:#555;'>The status of your ticket has been updated.</p>
                    <div style='background-color:#f8f9fa; padding:20px; border-left:4px solid #17a2b8; margin:20px 0;'>
                      <p><strong>Ticket Number:</strong> {ticketNumber}</p>
                      <p><strong>Title:</strong> {title}</p>
                      <p><strong>New Status:</strong> <span style='color:#17a2b8; font-weight:bold;'>{newStatus}</span></p>
                    </div>
                    <p style='text-align:center;'>
                      <a href='{ticketLink}' style='background:#17a2b8; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>View Ticket</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} HelpDesk Kyotera. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, $"Ticket {ticketNumber} Status Changed to {newStatus}", htmlContent, true);
        }

        public async Task SendTicketAssignedAsync(string toEmail, string recipientName, string ticketNumber, string title, string assignedBy, string ticketLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#ffc107;'>Ticket Assigned to You</h2>
                    <p style='font-size:16px; color:#555;'>Hello {recipientName},</p>
                    <p style='font-size:16px; color:#555;'>You have been assigned to the following ticket:</p>
                    <div style='background-color:#f8f9fa; padding:20px; border-left:4px solid #ffc107; margin:20px 0;'>
                      <p><strong>Ticket Number:</strong> {ticketNumber}</p>
                      <p><strong>Title:</strong> {title}</p>
                      <p><strong>Assigned By:</strong> {assignedBy}</p>
                    </div>
                    <p style='text-align:center;'>
                      <a href='{ticketLink}' style='background:#ffc107; color:#000; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>View Ticket</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} HelpDesk Kyotera. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, $"You Have Been Assigned to Ticket {ticketNumber}", htmlContent, true);
        }

        public async Task SendCommentAddedAsync(string toEmail, string recipientName, string ticketNumber, string commenterName, string ticketLink)
        {
            string htmlContent = $@"
                <html><body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0; padding:20px;'>
                  <div style='max-width:600px; margin:auto; background:#fff; padding:30px; border-radius:8px;'>
                    <h2 style='color:#28a745;'>New Comment on Ticket</h2>
                    <p style='font-size:16px; color:#555;'>Hello {recipientName},</p>
                    <p style='font-size:16px; color:#555;'>A new comment has been added to ticket {ticketNumber}.</p>
                    <div style='background-color:#f8f9fa; padding:20px; border-left:4px solid #28a745; margin:20px 0;'>
                      <p><strong>Ticket Number:</strong> {ticketNumber}</p>
                      <p><strong>Commented By:</strong> {commenterName}</p>
                    </div>
                    <p style='text-align:center;'>
                      <a href='{ticketLink}' style='background:#28a745; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; font-weight:bold;'>View Ticket</a>
                    </p>
                    <p style='font-size:12px; color:#999; margin-top:30px;'>&copy; {DateTime.UtcNow.Year} HelpDesk Kyotera. All rights reserved.</p>
                  </div>
                </body></html>";

            await SendEmailAsync(toEmail, $"New Comment on Ticket {ticketNumber}", htmlContent, true);
        }
    }
}