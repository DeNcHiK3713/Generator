using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    interface IOffsetLine
    {
        ulong Offset { get; set; }
        string Text { get; set; }
        string GetLine(StreamReader reader);
#if DEBUG
        long FindIndex(string[] lines);
#endif
    }
}
