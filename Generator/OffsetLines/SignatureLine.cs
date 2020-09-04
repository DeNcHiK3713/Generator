using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class SignatureLine : Line
    {
        public string Signature { get; set; }

        public override int FindOffset(ScriptJson scriptJson)
        {
            int index = -1;
            switch (Section)
            {
                case ScriptSection.ScriptMethod:
                    index = scriptJson.ScriptMethod.FindIndex(x => x.Signature == Signature);
                    Offset = scriptJson.ScriptMethod.ElementAt(index).Address;
                    break;
                case ScriptSection.ScriptMetadata:
                    index = scriptJson.ScriptMetadata.FindIndex(x => x.Signature == Signature);
                    Offset = scriptJson.ScriptMetadata.ElementAt(index).Address;
                    break;
            }
            return index;
        }
    }
}
