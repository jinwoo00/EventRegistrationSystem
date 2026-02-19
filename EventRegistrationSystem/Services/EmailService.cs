using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http; // 👈 Add this for IFormFile

namespace EventRegistrationSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];

                _logger.LogInformation($"Attempting to send email to {to}");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(to);

                await client.SendMailAsync(mail);
                _logger.LogInformation($"Email sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        // ========== NEW METHOD FOR ATTACHMENTS ==========
        public async Task SendWithAttachmentAsync(string to, string subject, string body, Stream attachmentStream, string fileName, string contentType)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var senderEmail = emailSettings["SenderEmail"];
                var senderName = emailSettings["SenderName"];

                _logger.LogInformation($"Sending email with attachment to {to}");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(to);

                if (attachmentStream != null && attachmentStream.Length > 0)
                {
                    // Ensure stream is at the beginning
                    attachmentStream.Position = 0;

                    // Create attachment from stream
                    var attachment = new Attachment(attachmentStream, fileName, contentType);
                    mail.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mail);
                _logger.LogInformation($"Email with attachment sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email with attachment to {to}");
                throw;
            }
        }
    }
}