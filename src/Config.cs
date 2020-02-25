using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Config
    {
        private const string FilePath = "appsettings.json";
        private readonly IConfiguration _configuration;

        public Config()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true)
                .AddEnvironmentVariables()
                .Build();

            DriverUrl = FindValue("DriverUrl");
            KaschusoUrl = FindValue("KaschusoUrl");
            KaschusoUsername = FindValue("KaschusoUsername");
            KaschusoPassword = FindValue("KaschusoPassword");
            GmailUsername = FindValue("GmailUsername");
            GmailPassword = FindValue("GmailPassword");
        }

        public string DriverUrl { get; }

        public string KaschusoUrl { get; }

        public string KaschusoUsername { get; }

        public string KaschusoPassword { get; }

        public string GmailUsername { get; }

        public string GmailPassword { get; }

        public string FindValue(string key)
        {
            return _configuration.GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
}