using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class CommentLine : ILine
    {
        public string Text { get; set; }

        public string GetLine(ScriptJson scriptJson)
        {
            return Text;
        }
    }
}
