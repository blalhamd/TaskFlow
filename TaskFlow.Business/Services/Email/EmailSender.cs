using TaskFlow.Core.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TaskFlow.Business.Services.Email
{
    public class EmailSender : IEmailSender // download Mailkit to send email by it alternative smtp
    {
        private readonly MailSettings _settings;

        public EmailSender(IOptionsSnapshot<MailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var message = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_settings.Email),
                    Subject = subject,
                };

                message.To.Add(MailboxAddress.Parse(email));

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };

                message.Body = builder.ToMessageBody();

                using var smtpClient = new SmtpClient(); // Note, Smtp that exist in MailKit

                smtpClient.Connect(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(_settings.Email, _settings.Password);

                await smtpClient.SendAsync(message);

                smtpClient.Disconnect(true);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
