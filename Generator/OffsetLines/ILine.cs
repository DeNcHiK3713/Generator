using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    interface ILine
    {
        string Text { get; set; }
        string GetLine(ScriptJson scriptJson);
    }
}
