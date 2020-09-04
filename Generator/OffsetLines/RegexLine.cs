using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class RegexLine : IOffsetLine
    {
        public ulong Offset { get; set; }
        public string Text { get; set; }
        public string Regex { get; set; }

        public void FindOffset(StreamReader reader)
        {
            var regex = new Regex($"      \"Name\": \"{Regex}\",");
            string line = null, previous;
            do
            {
                previous = line;
                line = reader.ReadLine();
            }
            while (!regex.IsMatch(line));
            Offset = ulong.Parse(previous.Substring(17, previous.Length - 18));
        }

        public string GetLine(StreamReader reader)
        {
            if (Offset == 0)
            {
                FindOffset(reader);
            }
            return $"#define {Text} {Offset}";
        }
#if DEBUG
        public long FindIndex(string[] lines)
        {
            var regex = new Regex($"      \"Name\": \"{Regex}\",");
            for (long l = 0, len = lines.LongLength; l < len; l++)
            {
                if (regex.IsMatch(lines[l]))
                {
                    return l;
                }
            }
            return -1;
        }
#endif
    }
}
