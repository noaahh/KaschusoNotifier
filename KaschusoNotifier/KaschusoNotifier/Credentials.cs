using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Credentials
    {
        private const string FilePath = "credentials.json";

        public static string Username { get; private set; }

        public static string Password { get; private set; }

        public static string MailUsername { get; private set; }

        public static string MailPassword { get; private set; }

        public static void Load()
        {
            IConfiguration credentials = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Username = credentials.GetChildren().FirstOrDefault(x => x.Key == "username")?.Value;
            Password = credentials.GetChildren().FirstOrDefault(x => x.Key == "password")?.Value;
            MailUsername = credentials.GetChildren().FirstOrDefault(x => x.Key == "mailUsername")?.Value;
            MailPassword = credentials.GetChildren().FirstOrDefault(x => x.Key == "mailPassword")?.Value;
        }
    }
}