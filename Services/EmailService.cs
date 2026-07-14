using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Services
{
    public class EmailService : IEmailService
    {
        public readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            string SenderName = _configuration["EmailSettings:SenderName"]!;
            string SenderEmail = _configuration["EmailSettings:SenderEmail"]!;

            email.From.Add(new MailboxAddress(SenderName,SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder{HtmlBody = htmlMessage};
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            string smtpServer = _configuration["EmailSettings:SmtpServer"]!;
            int port = int.Parse(_configuration["EmailSettings:Port"]!);
            string password = _configuration["EmailSettings:AppPassword"]!;

            await smtp.ConnectAsync(smtpServer,port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(SenderEmail,password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}