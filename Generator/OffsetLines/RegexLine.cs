using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class RegexLine : Line
    {
        public string Regex { get; set; }

        public override int FindOffset(ScriptJson scriptJson)
        {
            var regex = new Regex(Regex);

            int index = -1;
            switch (Section)
            {
                case ScriptSection.ScriptMethod:
                    index = scriptJson.ScriptMethod.FindIndex(x => regex.IsMatch(x.Name));
                    Offset = scriptJson.ScriptMethod.ElementAt(index).Address;
                    break;
                case ScriptSection.ScriptMetadata:
                    index = scriptJson.ScriptMetadata.FindIndex(x => regex.IsMatch(x.Name));
                    Offset = scriptJson.ScriptMethod.ElementAt(index).Address;
                    break;
            }
            return index;
        }
    }
}
