namespace Oxide.Plugins
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public partial class GUICreator
    {
        public class Command
        {
            const string flagSymbols = "/-";
            public string fullString;
            public string name;
            public List<string> args = new List<string>();
            public Dictionary<string, List<string>> flags = new Dictionary<string, List<string>>();

            public Command(string fullString)
            {
                this.fullString = fullString;
                string[] split = Regex.Split(fullString, " ");
                if (split.Length < 1) return;
                name = split.First();
                if (split.Length < 2) return;
                split = split.Skip(1).ToArray();
                List<string> flag = null;
                foreach (string arg in split)
                {
                    if (flagSymbols.Contains(arg.First()))
                    {
                        string sanitizedFlag = arg.Substring(1);
                        if (!flags.ContainsKey(sanitizedFlag))
                        {
                            flags.Add(sanitizedFlag, new List<string>());
                        }
                        flag = flags[sanitizedFlag];
                    }
                    else if (flag != null) flag.Add(arg);
                    else args.Add(arg);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder($"{name} ");
                foreach (string arg in args)
                {
                    sb.Append($"{arg} ");
                }
                foreach (string flag in flags.Keys)
                {
                    sb.Append($"\n{flag}: ");
                    foreach (string arg in flags[flag])
                    {
                        sb.Append($"{arg} ");
                    }
                }
                return sb.ToString();
            }
        }
    }
}