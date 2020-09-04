using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    interface IHasOffset
    {
        ulong Offset { get; set; }
        int FindOffset(ScriptJson scriptJson);
    }
}
