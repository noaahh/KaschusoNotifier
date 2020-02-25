using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using OpenQA.Selenium;
using Timer = System.Timers.Timer;

namespace KaschusoNotifier
{
    public class Notifier
    {
        private readonly Config _config;
        private readonly IWebDriver _driver;
        private readonly MailIssuer _mailIssuer;
        private readonly List<Mark> _marks = new List<Mark>();
        private readonly Timer _timer;

        public Notifier(IWebDriver driver, Config config)
        {
            _driver = driver;
            _config = config;
            _mailIssuer = new MailIssuer(_config);
            _timer = new Timer
            {
                AutoReset = false,
                Interval = 1000 * 60 // 1 minute
            };
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            if (!Login())
            {
                Console.WriteLine("Login failed. Please check your credentials.");
                Stop();
                return;
            }

            Console.WriteLine("Login successful");

            // Adding initial marks to discovered marks
            var initialMarks = GetNewMarks();
            _marks.AddRange(initialMarks);
            Console.WriteLine($"{initialMarks.Length} mark(s) already existing:");
            PrintMarks(initialMarks);

            Console.WriteLine("Starting frequent mark check");
            Console.WriteLine($"Interval: {_timer.Interval} ms");
            _timer.Start();
            _timer.Enabled = true;

            var autoEvent = new AutoResetEvent(false);
            autoEvent.WaitOne();
        }

        public void Stop()
        {
            Console.WriteLine("Stopping Notifier...");
            _timer.Stop();
            _driver.Quit();
            Console.WriteLine("Notifier stopped");
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Mark check triggered");
            try
            {
                CheckForNewMarks();
                _timer.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Stop();
            }
        }

        private void CheckForNewMarks()
        {
            var newMarks = GetNewMarks();
            Console.WriteLine($"{newMarks.Length} mark(s) discovered:");
            PrintMarks(newMarks);

            if (newMarks.Length > 0 && _mailIssuer.Notify(newMarks))
                _marks.AddRange(newMarks);
        }

        private Mark[] GetNewMarks()
        {
            _driver.Navigate().Refresh();

            var marksTable = _driver.FindElements(By.TagName("table"))[2];

            var rows = marksTable.FindElements(By.XPath(".//tbody/tr"));
            if (rows.Any(x => x.Text.Contains("Sie haben alle"))) return new Mark[0];
            var newMarks = new List<Mark>();
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.XPath(".//td"));
                if (cells.All(x => _marks.All(y => y.Name != x.Text)))
                    newMarks.Add(new Mark(cells[0].Text, cells[1].Text, cells[3].Text));
            }

            return newMarks.ToArray();
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

        public void PrintMarks(Mark[] marks)
        {
            foreach (var mark in marks)
                Console.WriteLine($"\t{mark.Subject} | {mark.Name} | {mark.Value}");
        }
    }
}