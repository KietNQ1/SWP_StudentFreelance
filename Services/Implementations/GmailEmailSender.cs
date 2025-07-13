using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StudentFreelance.Models.Email;
using StudentFreelance.Services.Interfaces;

namespace StudentFreelance.Services.Implementations
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public GmailEmailSender(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {   
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                client.EnableSsl = true;

                Console.WriteLine($"Đang gửi mail tới {toEmail} qua SMTP: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");

                await client.SendMailAsync(message);

                Console.WriteLine("Gửi email thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI GỬI EMAIL: " + ex.ToString());
                throw;
            }
        }

    }
}
