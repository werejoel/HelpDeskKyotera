# Email Notifications Implementation Guide

## Configuration

### 1. Update appsettings.json

Add your SMTP credentials to `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "HelpDesk Kyotera",
  "Password": "your-app-password"
}
```

**For Gmail:**
- Use App Password (not regular password)
- Enable "Less secure app access" if needed
- Or use OAuth2 for better security

### 2. Services Already Registered

In `Program.cs`:
```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Available Email Methods

### EmailService Methods:
- `SendNewTicketNotificationAsync()` - Notify when ticket is created
- `SendTicketStatusChangedAsync()` - Notify when ticket status changes
- `SendTicketAssignedAsync()` - Notify when ticket is assigned
- `SendCommentAddedAsync()` - Notify when comment is added
- `SendHtmlEmailAsync()` - Generic HTML email sender

### NotificationService Methods:
- `CreateNotificationAsync()` - Save to DB only
- `CreateNotificationAndSendEmailAsync()` - Save to DB AND send email
- `GetUserNotificationsAsync()` - Retrieve user notifications
- `MarkAsReadAsync()` - Mark as read

## Integration Examples

### Example 1: Send Email When Ticket is Created

In `TicketsController` Create method:

```csharp
[HttpPost]
public async Task<IActionResult> Create(Ticket ticket)
{
    if (ModelState.IsValid)
    {
        ticket.TicketId = Guid.NewGuid();
        ticket.CreatedOn = DateTime.UtcNow;
        ticket.CreatedBy = GetCurrentUserId();
        
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        
        // Get requester details
        var requester = await _context.Users.FindAsync(ticket.CreatedBy);
        
        // Build ticket link
        var ticketLink = $"{Request.Scheme}://{Request.Host}/Tickets/Details/{ticket.TicketId}";
        
        // Get admin emails to notify them
        var admins = await _context.Users
            .Where(u => u.UserRoles.Any(r => r.Role.Name == "Admin"))
            .Select(u => u.Email)
            .ToListAsync();
        
        // Send email to each admin
        foreach (var adminEmail in admins)
        {
            await _emailService.SendNewTicketNotificationAsync(
                toEmail: adminEmail,
                recipientName: "Admin",
                ticketNumber: ticket.TicketNumber,
                title: ticket.Title,
                requesterName: requester?.UserName ?? "Unknown",
                ticketLink: ticketLink
            );
        }
        
        TempData["Success"] = "Ticket created successfully!";
        return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
    }
    
    return View(ticket);
}
```

### Example 2: Send Email When Status Changes

```csharp
[HttpPost]
public async Task<IActionResult> ChangeStatus(Guid ticketId, Guid statusId)
{
    var ticket = await _context.Tickets
        .Include(t => t.Status)
        .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    
    if (ticket == null) return NotFound();
    
    var oldStatus = ticket.Status?.Name ?? "Unknown";
    var newStatus = await _context.Status.FindAsync(statusId);
    
    ticket.StatusId = statusId;
    ticket.UpdatedOn = DateTime.UtcNow;
    
    _context.Tickets.Update(ticket);
    await _context.SaveChangesAsync();
    
    // Get ticket creator's email
    var creator = await _context.Users.FindAsync(ticket.CreatedBy);
    if (creator?.Email != null)
    {
        var ticketLink = $"{Request.Scheme}://{Request.Host}/Tickets/Details/{ticket.TicketId}";
        
        await _emailService.SendTicketStatusChangedAsync(
            toEmail: creator.Email,
            recipientName: creator.UserName,
            ticketNumber: ticket.TicketNumber,
            title: ticket.Title,
            newStatus: newStatus?.Name ?? "Unknown",
            ticketLink: ticketLink
        );
    }
    
    TempData["Success"] = "Status updated successfully!";
    return RedirectToAction(nameof(Details), new { id = ticketId });
}
```

### Example 3: Send Email When Assigned

```csharp
[HttpPost]
public async Task<IActionResult> AssignTicket(Guid ticketId, Guid assignToUserId)
{
    var ticket = await _context.Tickets.FindAsync(ticketId);
    var assignedTo = await _context.Users.FindAsync(assignToUserId);
    var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
    
    if (ticket == null || assignedTo == null) return NotFound();
    
    ticket.AssignedToId = assignToUserId;
    ticket.UpdatedOn = DateTime.UtcNow;
    
    _context.Tickets.Update(ticket);
    await _context.SaveChangesAsync();
    
    // Send email to assigned user
    if (assignedTo.Email != null)
    {
        var ticketLink = $"{Request.Scheme}://{Request.Host}/Tickets/Details/{ticket.TicketId}";
        
        await _emailService.SendTicketAssignedAsync(
            toEmail: assignedTo.Email,
            recipientName: assignedTo.UserName,
            ticketNumber: ticket.TicketNumber,
            title: ticket.Title,
            assignedBy: currentUser?.UserName ?? "System",
            ticketLink: ticketLink
        );
    }
    
    TempData["Success"] = "Ticket assigned successfully!";
    return RedirectToAction(nameof(Details), new { id = ticketId });
}
```

### Example 4: Send Email When Comment is Added

```csharp
[HttpPost]
public async Task<IActionResult> AddComment(Guid ticketId, string commentText)
{
    var ticket = await _context.Tickets
        .Include(t => t.Comments)
        .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    
    if (ticket == null) return NotFound();
    
    var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
    
    var comment = new Comment
    {
        CommentId = Guid.NewGuid(),
        TicketId = ticketId,
        CommentText = commentText,
        CreatedBy = GetCurrentUserId(),
        CreatedOn = DateTime.UtcNow
    };
    
    _context.Comments.Add(comment);
    await _context.SaveChangesAsync();
    
    // Notify ticket creator and assigned user
    var usersToNotify = new List<ApplicationUser>();
    
    var creator = await _context.Users.FindAsync(ticket.CreatedBy);
    if (creator != null) usersToNotify.Add(creator);
    
    if (ticket.AssignedToId.HasValue)
    {
        var assignee = await _context.Users.FindAsync(ticket.AssignedToId);
        if (assignee != null) usersToNotify.Add(assignee);
    }
    
    // Send emails
    var ticketLink = $"{Request.Scheme}://{Request.Host}/Tickets/Details/{ticket.TicketId}";
    
    foreach (var user in usersToNotify)
    {
        if (user.Email != null && user.Id != GetCurrentUserId()) // Don't notify commenter
        {
            await _emailService.SendCommentAddedAsync(
                toEmail: user.Email,
                recipientName: user.UserName,
                ticketNumber: ticket.TicketNumber,
                commenterName: currentUser?.UserName ?? "Unknown",
                ticketLink: ticketLink
            );
        }
    }
    
    TempData["Success"] = "Comment added successfully!";
    return RedirectToAction(nameof(Details), new { id = ticketId });
}
```

### Example 5: Using NotificationService with Email

For database tracking + email:

```csharp
await _notificationService.CreateNotificationAndSendEmailAsync(
    userId: targetUserId,
    email: user.Email,
    subject: "New Ticket Assigned",
    body: $@"
        <html><body>
            <p>Hello {user.UserName},</p>
            <p>You have been assigned to ticket <strong>{ticket.TicketNumber}</strong></p>
            <a href='{ticketLink}'>View Ticket</a>
        </body></html>",
    link: ticketLink
);
```

## Best Practices

1. **Always include try-catch** when sending emails (email failures shouldn't crash your app)
2. **Build full ticket links** using `Request.Scheme` and `Request.Host`
3. **Load related data** (User, Status, etc.) before building email content
4. **Use meaningful ticket links** so users can click directly to tickets
5. **Batch notifications** - Consider queuing for high-volume scenarios
6. **Test emails** - Use a test account before production
7. **Handle null emails** - Not all users may have email addresses
8. **Log failures** - Add logging for email sending errors

## Testing in Development

For development/testing without real SMTP:

1. Use **Mailtrap** (https://mailtrap.io) - captures emails in inbox
2. Use **MailHog** - local SMTP server for testing
3. Use **Console** - log emails instead of sending

Update appsettings in testing:
```json
"EmailSettings": {
  "SmtpServer": "smtp.mailtrap.io",
  "SmtpPort": "2525",
  "SenderEmail": "your-mailtrap-email",
  "SenderName": "HelpDesk Test",
  "Password": "your-mailtrap-password"
}
```

## Troubleshooting

**Emails not sending?**
- Check SMTP credentials in appsettings.json
- Verify firewall allows SMTP port (587 or 465)
- Check email provider's app-specific password requirements
- Review application logs for exceptions

**Gmail not working?**
- Use 16-character App Password, not regular password
- Enable "Less secure app access" (not recommended)
- Consider using OAuth2 instead

**Production Considerations**
- Use environment-specific appsettings files
- Never commit real credentials to source control
- Use Azure Key Vault or similar for secrets management
- Consider async background job processing (Hangfire, etc.)
- Monitor email delivery and bounces
