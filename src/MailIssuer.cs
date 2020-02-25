using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace KaschusoNotifier
{
    public class MailIssuer
    {
        private const string MailSubject = "KaschusoNotifier";
        private readonly Config _config;

        public MailIssuer(Config config)
        {
            _config = config;
        }

        public bool Notify(Mark[] marks)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                // UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_config.GmailUsername, _config.GmailPassword),
                Timeout = 20000
            };
            var mailMessage = new MailMessage(_config.GmailUsername, _config.GmailUsername)
            {
                Subject = MailSubject,
                Body = GenerateBody(marks)
            };

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("Mail delivery failed. Check your Gmail credentials.");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public static string GenerateBody(Mark[] marks)
        {
            return marks.Aggregate("", (current, mark) => current + $"{mark.Subject} | {mark.Name}: {mark.Value}\n\n");
        }
    }
}