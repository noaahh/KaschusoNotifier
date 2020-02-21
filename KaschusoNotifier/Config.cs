using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Config
    {
        private const string FilePath = "config.json";

        private readonly IConfiguration configuration;

        public bool Headless { get; private set; }

        public string URL { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public string MailUsername { get; private set; }

        public string MailPassword { get; private set; }

        public Config()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Headless = Convert.ToBoolean(FindValue("headless"));
            URL = FindValue("url");
            Username = FindValue("username");
            Password = FindValue("password");
            MailUsername = FindValue("mailUsername");
            MailPassword = FindValue("mailPassword");
        }

        public string FindValue(string key)
        {
            return configuration.GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
}