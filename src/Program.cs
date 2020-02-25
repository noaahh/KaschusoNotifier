using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace KaschusoNotifier
{
    public class Program
    {
        private static readonly Config Config = new Config();
        private static Notifier _notifier;

        public static void Main(string[] args)
        {
            Console.WriteLine("Creating default Notifier");
            _notifier = new Notifier(CreateWebDriver(), Config);

            Console.WriteLine("Starting Notifier");
            _notifier.Start();
        }

        private static IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            return Config.DriverUrl == "driver"
                ? new ChromeDriver(Config.DriverUrl, options)
                : new RemoteWebDriver(new Uri(Config.DriverUrl), options);
        }
    }
}