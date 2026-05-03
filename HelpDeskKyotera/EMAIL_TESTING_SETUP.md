# Email Testing Setup Guide

## Quick Setup for Development

### Option 1: Mailtrap (Recommended for Development)

Mailtrap is a free service that captures all emails sent from your application. Perfect for testing.

#### Step 1: Create Mailtrap Account
1. Go to https://mailtrap.io
2. Click "Sign up" and create a free account
3. Verify your email

#### Step 2: Get Your Credentials
1. Log in to Mailtrap
2. Go to **Email Testing → Inboxes**
3. Select the default inbox or create a new one
4. Click on the inbox name
5. You'll see integration options - click on **"Integrator"** tab
6. Select **".NET"** or scroll to find SMTP settings
7. You'll see:
   - **Host**: smtp.mailtrap.io
   - **Port**: 2525 (or 465/587)
   - **Username**: your-mailtrap-username (looks like a UUID)
   - **Password**: your-mailtrap-password

#### Step 3: Update appsettings.json

Replace your current EmailSettings with your Mailtrap credentials:

```json
"EmailSettings": {
  "SmtpServer": "smtp.mailtrap.io",
  "SmtpPort": "2525",
  "SenderEmail": "your-mailtrap-username",
  "SenderName": "HelpDesk Kyotera (Dev)",
  "Password": "your-mailtrap-password"
}
```

#### Step 4: Test Email Sending

Run your application and trigger an action that sends an email (e.g., create a ticket). Check your Mailtrap inbox to see the email.

### Option 2: MailHog (Local Alternative)

MailHog is a local SMTP server that runs on your machine - no account needed.

#### Step 1: Install MailHog

**Windows (using Chocolatey):**
```powershell
choco install mailhog
```

**Windows (Manual):**
1. Download from https://github.com/mailhog/MailHog/releases
2. Extract and run `MailHog.exe`

**macOS (using Homebrew):**
```bash
brew install mailhog
brew services start mailhog
```

**Docker:**
```bash
docker run --rm -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

#### Step 2: Configure Your App

Update appsettings.json:

```json
"EmailSettings": {
  "SmtpServer": "localhost",
  "SmtpPort": "1025",
  "SenderEmail": "test@example.com",
  "SenderName": "HelpDesk Kyotera (Dev)",
  "Password": "password"
}
```

#### Step 3: Access MailHog UI

Open your browser and go to: **http://localhost:8025**

All emails sent will appear in this web interface.

### Option 3: Gmail (Production Alternative)

For testing with Gmail, use an App Password (2-factor auth required).

#### Step 1: Enable 2-Factor Authentication
1. Go to https://myaccount.google.com/security
2. Enable 2-Step Verification

#### Step 2: Create App Password
1. Go to https://myaccount.google.com/apppasswords
2. Select "Mail" and "Windows Computer"
3. Google will generate a 16-character password

#### Step 3: Update appsettings.json

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "your-gmail@gmail.com",
  "SenderName": "HelpDesk Kyotera",
  "Password": "your-16-char-app-password"
}
```

## Testing Email Functionality

### Manual Testing Steps

1. **Test New Ticket Notification**
   - Log in as a regular user
   - Create a new ticket
   - Check Mailtrap/MailHog inbox for notification to admins
   - Expected: Email from "HelpDesk Kyotera" with ticket details

2. **Test Status Change Notification**
   - Log in as an admin
   - Go to a ticket → Change status
   - Check inbox for notification to ticket creator/assignee
   - Expected: Email with updated status

3. **Test Assignment Notification**
   - Log in as an admin
   - Go to a ticket → Assign to user
   - Check inbox for notification
   - Expected: Email to assigned user

### Automated Testing (Optional)

Create a simple test method in your project:

```csharp
[Test]
public async Task TestEmailService()
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
    
    var emailService = new EmailService(config);
    
    await emailService.SendNewTicketNotificationAsync(
        toEmail: "test@example.com",
        recipientName: "Test User",
        ticketNumber: "TK-001",
        title: "Test Ticket",
        requesterName: "John Doe",
        ticketLink: "http://localhost:5128/Tickets/Details/12345"
    );
}
```

## Troubleshooting

### Email not sending?

1. **Check SMTP credentials**
   - Verify username/password in appsettings.json
   - Test with Mailtrap first (easiest)

2. **Check firewall**
   - Port 2525, 587, or 1025 might be blocked
   - Try port 465 (SSL) if 587 doesn't work

3. **Check logs**
   - Look for exceptions in Visual Studio Output window
   - Check Application Insights or local logging

4. **Test SMTP connection** (PowerShell):
   ```powershell
   $smtp = New-Object Net.Mail.SmtpClient("smtp.mailtrap.io", 2525)
   $smtp.EnableSsl = $true
   $smtp.Credentials = New-Object System.Net.NetworkCredential("username", "password")
   $smtp.Send("from@example.com", "to@example.com", "Test", "Test Body")
   ```

### Email template issues?

- Ensure HTML in email templates is properly formatted
- Test in email client to see rendering
- Check for special characters that might need escaping

### Gmail not accepting password?

- Make sure you're using App Password (16 chars), not regular password
- Verify 2-Factor Authentication is enabled
- Try "Less secure app access" setting (not recommended long-term)

## Production Setup Recommendations

1. **Use Azure SendGrid or AWS SES** (not Gmail)
   - More reliable for production
   - Better deliverability
   - Track bounces and complaints

2. **Store credentials securely**
   - Never commit credentials to source control
   - Use Azure Key Vault or similar
   - Use environment variables for sensitive data

3. **Monitor email delivery**
   - Log all sent emails
   - Track bounce rates
   - Set up alerts for failures

4. **Consider async processing**
   - Use Hangfire or Background Service for email sending
   - Prevents email failures from blocking user requests
   - Allows retry logic

### Example: Storing in User Secrets (Local Dev)

```bash
# Set the secret
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.mailtrap.io"
dotnet user-secrets set "EmailSettings:Password" "your-password"

# View all secrets
dotnet user-secrets list
```

Then use User Secrets in development:
```csharp
// In Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
```

## Verification Checklist

- [ ] Mailtrap/MailHog account created and running
- [ ] SMTP credentials added to appsettings.json
- [ ] EmailSettings section has all required fields
- [ ] Application compiles without errors
- [ ] Can create a ticket successfully
- [ ] Email appears in Mailtrap/MailHog inbox
- [ ] Email contains correct information
- [ ] All notification types tested (create, assign, status change)
