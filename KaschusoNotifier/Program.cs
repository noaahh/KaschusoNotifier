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
        private readonly Config config = new Config();

        private readonly MailIssuer MailIssuer = new MailIssuer();

        private readonly List<Mark> UnconfirmedMarks = new List<Mark>();

        private readonly IWebDriver Driver;

        public Program() => Driver = CreateWebDriver(config.Headless);

        private static void Main(string[] args) => new Program().Run();

        public void Run()
        {
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

        private bool Login()
        {
            Driver.Url = config.URL;
            var userIdControl = Driver.FindElement(By.Name("userid"));
            userIdControl.Click();
            userIdControl.SendKeys(config.Username);

            var passwordControl = Driver.FindElement(By.Name("password"));
            passwordControl.Click();
            passwordControl.SendKeys(config.Password);
            passwordControl.Submit();
            return Driver.Title == "schulNetz";
        }

        private Mark[] GetUnconfirmedMarks()
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

        private IWebDriver CreateWebDriver(bool headless)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArgument("headless");
            return new ChromeDriver("driver", options);
        }
    }
}