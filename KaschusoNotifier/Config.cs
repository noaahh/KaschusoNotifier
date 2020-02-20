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

        public static string URL { get; private set; }

        public static void Load()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(FilePath, true, true)
                .Build();
            Headless = Convert.ToBoolean(FindValue(config, "headless"));
            ToAddress = FindValue(config, "toAddress");
            URL = FindValue(config, "url");
        }

        public static string FindValue(IConfiguration config, string key)
        {
            return config.GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
}