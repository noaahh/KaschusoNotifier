using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Config
    {
        private const string FilePath = "config.json";

        public static string ToAddress { get; private set; }

        public static void Load()
        {
            IConfiguration credentials = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            ToAddress = credentials.GetChildren().FirstOrDefault(x => x.Key == "toAddress")?.Value;
        }
    }
}