using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class SignatureLine : IOffsetLine
    {
        public ulong Offset { get; set; }
        public string Text { get; set; }
        public string Signature { get; set; }

        public void FindOffset(StreamReader reader)
        {
            var match = $"      \"Signature\": \"{Signature}\"";
            string line = null, previous = null, previous2;
            do
            {
                previous2 = previous;
                previous = line;
                line = reader.ReadLine();
            }
            while (match != line);
            Offset = ulong.Parse(previous2.Substring(17, previous2.Length - 18));
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
            var match = $"      \"Signature\": \"{Signature}\"";
            for (long l = 0, len = lines.LongLength; l < len; l++)
            {
                if (lines[l] == match)
                {
                    return l;
                }
            }
            return -1;
        }
#endif
    }
}
