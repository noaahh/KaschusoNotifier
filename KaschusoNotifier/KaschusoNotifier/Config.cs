using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace KaschusoNotifier
{
    public class Config
    {
        private const string FilePath = "config.json";

        public static bool Headless { get; private set; }

        public static string ToAddress { get; private set; }

        public static void Load()
        {
            IConfiguration credentials = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Headless = Convert.ToBoolean(credentials.GetChildren().FirstOrDefault(x => x.Key == "headless")?.Value);
            ToAddress = credentials.GetChildren().FirstOrDefault(x => x.Key == "toAddress")?.Value;
        }
    }
}