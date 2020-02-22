using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace KaschusoNotifier
{
    public class Notifier
    {
        private readonly MailIssuer _mailIssuer = new MailIssuer();
        private readonly List<Mark> _marks = new List<Mark>();
        private readonly IWebDriver _driver;
        private readonly Config _config;

        public Notifier(IWebDriver driver, Config config)
        {
            _driver = driver;
            _config = config;
        }

        public void Run()
        {
            if (!Login())
            {
                Console.WriteLine("Login failed. Please check your credentials");
                _driver.Quit();
                return;
            }

            Console.WriteLine("Login successful");

            _marks.AddRange(GetNewMarks());

            var autoEvent = new AutoResetEvent(false);
            new Timer(this.CheckForNewMarks, autoEvent, 5000, 60000);
            autoEvent.WaitOne();
        }

        private void CheckForNewMarks(object stateInfo)
        {
            var newMarks = GetNewMarks();
            foreach (var newMark in newMarks) Console.WriteLine(newMark.Name);
            if (newMarks.Length > 0 && _mailIssuer.Notify(newMarks))
            {
                _marks.AddRange(newMarks);
            }
            else
            {
                Console.WriteLine("No new marks found");
            }
        }

        private bool Login()
        {
            _driver.Url = _config.KaschusoUrl;
            var userIdControl = _driver.FindElement(By.Name("userid"));
            userIdControl.Click();
            userIdControl.SendKeys(_config.KaschusoUsername);

            var passwordControl = _driver.FindElement(By.Name("password"));
            passwordControl.Click();
            passwordControl.SendKeys(_config.KaschusoPassword);
            passwordControl.Submit();
            return _driver.Title == "schulNetz";
        }

        private Mark[] GetNewMarks()
        {
            _driver.Navigate().Refresh();

            var marksTable = _driver.FindElements(By.TagName("table"))[2];

            var rows = marksTable.FindElements(By.XPath(".//tbody/tr"));
            if (rows.Any(x => x.Text.Contains("Sie haben alle")))
            {
                return new Mark[0];
            }
            var newMarks = new List<Mark>();
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.XPath(".//td"));
                if (cells.All(x => _marks.All(y => y.Subject != x.Text)))
                    newMarks.Add(new Mark(cells[0].Text, cells[1].Text, cells[3].Text));
            }
            return newMarks.ToArray();
        }
    }
}
