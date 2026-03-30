using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace HospitalRoomAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string to, string subject, string body)
        {
            var email = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]!);

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true
            };

            // ✅ FIX: correct variable name
            var mail = new MailMessage(email, to, subject, body);

            client.Send(mail);
        }
    }
}