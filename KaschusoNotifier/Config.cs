using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Config
    {
        private const string FilePath = "config.json";

        public static bool Headless { get; private set; }

        public static string URL { get; private set; }

        public static string Username { get; private set; }

        public static string Password { get; private set; }

        public static string MailUsername { get; private set; }

        public static string MailPassword { get; private set; }

        public static void Load()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Headless = Convert.ToBoolean(FindValue(config, "headless"));
            URL = FindValue(config, "url");
            Username = FindValue(config, "username");
            Password = FindValue(config, "password");
            MailUsername = FindValue(config, "mailUsername");
            MailPassword = FindValue(config, "mailPassword");
        }

        public static string FindValue(IConfiguration config, string key)
        {
            return config.GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
}