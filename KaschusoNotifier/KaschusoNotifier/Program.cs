using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace KaschusoNotifier
{
    internal class Program
    {
        private static readonly MailIssuer MailIssuer = new MailIssuer();
        public static IWebDriver Driver { get; private set; }

        private static void Main(string[] args)
        {
            Config.Load();
            Credentials.Load();
            Driver = CreateWebDriver(true);
            if (!Login())
            {
                Console.WriteLine("Login failed. Please check your credentials");
                Driver.Quit();
                return;
            }

            Console.WriteLine("Login successful");

            while (true)
                if (!IsUnconfirmedMarkAvailable())
                {
                    Console.WriteLine("No unconfirmed mark found");
                    Thread.Sleep(5000);
                    Driver.Navigate().Refresh();
                }
                else
                {
                    MailIssuer.Notify(new[] {new Mark("Test", "6.0"), new Mark("Test2", "1.0")});
                }
        }

        private static bool Login()
        {
            Driver.Url = "https://kaschuso.so.ch/gibsso";
            var userIdControl = Driver.FindElement(By.Name("userid"));
            userIdControl.Click();
            userIdControl.SendKeys(Credentials.Username);

            var passwordControl = Driver.FindElement(By.Name("password"));
            passwordControl.Click();
            passwordControl.SendKeys(Credentials.Password);
            passwordControl.Submit();
            return Driver.Title == "schulNetz";
        }

        private static bool IsUnconfirmedMarkAvailable()
        {
            var webElements = Driver.FindElements(
                By.TagName("span"));
            return webElements.FirstOrDefault(x => x.Text.Equals("Sie haben alle Noten bestätigt.")) == null;
        }

        private static IWebDriver CreateWebDriver(bool headless)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArgument("headless");
            return new ChromeDriver("driver", options);
        }
    }
}