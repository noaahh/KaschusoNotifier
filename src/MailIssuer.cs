using System;
using System.Net;
using System.Net.Mail;

namespace KaschusoNotifier
{
    public class MailIssuer
    {
        private readonly Config config = new Config();

        private const string MailSubject = "KaschusoNotifier";

        public bool Notify(Mark[] marks)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(config.GmailUsername, config.GmailPassword),
                Timeout = 20000
            };
            var message = new MailMessage(config.GmailUsername, config.GmailUsername)
            {
                Subject = MailSubject,
                Body = Mark.GenerateBody(marks)
            };
            try
            {
                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}