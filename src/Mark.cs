using System.Linq;

namespace KaschusoNotifier
{
    public class Mark
    {
        public Mark(string subject, string name, string value)
        {
            Subject = subject;
            Name = name;
            Value = value;
        }

        public string Subject { get; }

        public string Name { get; }

        public string Value { get; }
    }
}