using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace KaschusoNotifier
{
    internal class Program
    {
        private static readonly MailIssuer MailIssuer = new MailIssuer();

        private static readonly List<Mark> UnconfirmedMarks = new List<Mark>();
        public static IWebDriver Driver { get; private set; }

        private static void Main(string[] args)
        {
            Config.Load();
            Driver = CreateWebDriver(Config.Headless);
            if (!Login())
            {
                Console.WriteLine("Login failed. Please check your credentials");
                Driver.Quit();
                return;
            }

            Console.WriteLine("Login successful");

            while (true)
            {
                var unconfirmedMarks = GetUnconfirmedMarks();
                foreach (var unconfirmedMark in unconfirmedMarks) Console.WriteLine(unconfirmedMark.Name);
                if (unconfirmedMarks.Length > 0)
                {
                    MailIssuer.Notify(unconfirmedMarks);
                }
                else
                {
                    Console.WriteLine("No unconfirmed mark found");
                    Thread.Sleep(5000);
                    Driver.Navigate().Refresh();
                }
            }
        }

        private static bool Login()
        {
            Driver.Url = Config.URL;
            var userIdControl = Driver.FindElement(By.Name("userid"));
            userIdControl.Click();
            userIdControl.SendKeys(Config.Username);

            var passwordControl = Driver.FindElement(By.Name("password"));
            passwordControl.Click();
            passwordControl.SendKeys(Config.Password);
            passwordControl.Submit();
            return Driver.Title == "schulNetz";
        }

        private static Mark[] GetUnconfirmedMarks()
        {
            var unconfirmedTables =
                Driver.FindElements(By.TagName("table"));

            var unconfirmedTable = unconfirmedTables[2];
            var rows = unconfirmedTable.FindElements(By.XPath(".//tbody/tr"));
            if (rows.Any(x => x.Text.Contains("Sie haben alle")))
            {
                return new Mark[0];
            }
            var discoveredMarks = new List<Mark>();
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.XPath(".//td"));
                if (cells.All(x => UnconfirmedMarks.All(y => y.Subject != x.Text)))
                    discoveredMarks.Add(new Mark(cells[0].Text, cells[1].Text, cells[3].Text));
            }

            UnconfirmedMarks.AddRange(discoveredMarks);
            return discoveredMarks.ToArray();
        }

        private static IWebDriver CreateWebDriver(bool headless)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArgument("headless");
            return new ChromeDriver("driver", options);
        }
    }
}