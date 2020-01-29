using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Credentials
    {
        private const string FilePath = "credentials.json";

        public static string Username { get; set; }

        public static string Password { get; set; }

        public static void Load()
        {
            IConfiguration credentials = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Username = credentials.GetChildren().FirstOrDefault(x => x.Key == "username")?.Value;
            Password = credentials.GetChildren().FirstOrDefault(x => x.Key == "password")?.Value;
        }
    }
}
