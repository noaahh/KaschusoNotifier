using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace KaschusoNotifier
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Config();
            new Notifier(CreateWebDriver(config), config).Run();
        }

        private static IWebDriver CreateWebDriver(Config config)
        {
            Console.WriteLine(config.DriverUrl);
            Console.WriteLine(config.KaschusoUsername);
            var options = new ChromeOptions();
            return config.DriverUrl == "driver" 
                ? new ChromeDriver(config.DriverUrl, options) 
                : new RemoteWebDriver(new Uri(config.DriverUrl), options);
        }
    }
}