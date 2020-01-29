using System.Linq;

namespace KaschusoNotifier
{
    public class Mark
    {
        public Mark(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public static string GenerateBody(Mark[] marks)
        {
            return marks.Aggregate("", (current, mark) => current + $"{mark.Name}: {mark.Value}\n");
        }
    }
}