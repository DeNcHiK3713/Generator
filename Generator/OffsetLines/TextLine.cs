using Il2CppDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    class TextLine : Line
    {
        public string Name { get; set; }

        public override int FindOffset(ScriptJson scriptJson)
        {
            int index = -1;
            switch (Section)
            {
                case ScriptSection.ScriptMethod:
                    index = scriptJson.ScriptMethod.FindIndex(x => x.Name == Name);
                    Offset = scriptJson.ScriptMethod.ElementAt(index).Address;
                    break;
                case ScriptSection.ScriptMetadata:
                    index = scriptJson.ScriptMetadata.FindIndex(x => x.Name == Name);
                    Offset = scriptJson.ScriptMetadata.ElementAt(index).Address;
                    break;
                case ScriptSection.ScriptMetadataMethod:
                    index = scriptJson.ScriptMetadataMethod.FindIndex(x => x.Name == Name);
                    Offset = scriptJson.ScriptMetadataMethod.ElementAt(index).Address;
                    break;
            }
            return index;
        }
    }
}
