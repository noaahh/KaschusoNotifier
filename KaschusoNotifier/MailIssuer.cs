using System;
using System.Net;
using System.Net.Mail;

namespace KaschusoNotifier
{
    public class MailIssuer
    {
        private const string MailSubject = "KaschusoNotifier";

        public bool Notify(Mark[] marks)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(Config.MailUsername, Config.MailPassword),
                Timeout = 20000
            };
            var message = new MailMessage(Config.MailUsername, Config.MailUsername)
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