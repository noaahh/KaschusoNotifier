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

        private readonly List<Mark> marks = new List<Mark>();

        private readonly IWebDriver driver;

        public Program() => driver = CreateWebDriver(config.Headless);

        private static void Main(string[] args) => new Program().Run();

        public void Run()
        {
            if (!Login())
            {
                Console.WriteLine("Login failed. Please check your credentials");
                driver.Quit();
                return;
            }

            Console.WriteLine("Login successful");
            
            marks.AddRange(GetNewMarks());

            while (true)
            {
                var newMarks = GetNewMarks();
                foreach (var newMark in newMarks) Console.WriteLine(newMark.Name);
                if (newMarks.Length > 0 && MailIssuer.Notify(newMarks))
                {
                    marks.AddRange(newMarks);
                }
                else
                {
                    Console.WriteLine("No new marks found");
                    Thread.Sleep(5000);
                }
            }
        }

        private bool Login()
        {
            driver.Url = config.URL;
            var userIdControl = driver.FindElement(By.Name("userid"));
            userIdControl.Click();
            userIdControl.SendKeys(config.Username);

            var passwordControl = driver.FindElement(By.Name("password"));
            passwordControl.Click();
            passwordControl.SendKeys(config.Password);
            passwordControl.Submit();
            return driver.Title == "schulNetz";
        }

        private Mark[] GetNewMarks()
        {
            driver.Navigate().Refresh();
         
            var marksTable = driver.FindElements(By.TagName("table"))[2];

            var rows = marksTable.FindElements(By.XPath(".//tbody/tr"));
            if (rows.Any(x => x.Text.Contains("Sie haben alle")))
            {
                return new Mark[0];
            }
            var newMarks = new List<Mark>();
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.XPath(".//td"));
                if (cells.All(x => marks.All(y => y.Subject != x.Text)))
                    newMarks.Add(new Mark(cells[0].Text, cells[1].Text, cells[3].Text));
            }
            return newMarks.ToArray();
        }

        private IWebDriver CreateWebDriver(bool headless)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArgument("headless");
            return new ChromeDriver("driver", options);
        }
    }
}