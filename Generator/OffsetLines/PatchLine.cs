using Il2CppDumper;
using Keystone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.OffsetLines
{
    abstract class PatchLine : ILine, IHasOffset
    {
        public ulong Offset { get; set; }
        public string Text { get; set; }

        protected Il2CppAddressConverter _addressConverter;

        protected ulong startOffset;
        protected ulong endOffset;
        public Line Target { get; set; }
        public byte[] PatchData { get; set; }
        public virtual void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture, Il2CppAddressConverter addressConverter)
        {
            _addressConverter = addressConverter;
            FindOffset(scriptJson);
        }

        public int FindOffset(ScriptJson scriptJson)
        {
            var index = Target.FindOffset(scriptJson);
            if (index != -1)
            {
                startOffset = Target.Offset;
                switch (Target.Section)
                {
                    case ScriptSection.ScriptMethod:
                        if (index + 1 < scriptJson.ScriptMethod.Count)
                        {
                            endOffset = scriptJson.ScriptMethod.ElementAt(index + 1).Address - 4;
                        }
                        break;
                    case ScriptSection.ScriptMetadata:
                        if (index + 1 < scriptJson.ScriptMetadata.Count)
                        {
                            endOffset = scriptJson.ScriptMetadata.ElementAt(index + 1).Address - 4;
                        }
                        break;
                }
            }

            if (_addressConverter is not null)
            {
                startOffset = _addressConverter.RvaToOffset(startOffset);
                endOffset = _addressConverter.RvaToOffset(endOffset);
            }

            return index;
        }

        public string GetLine(ScriptJson scriptJson)
        {
            if (_addressConverter is not null)
            {
                Offset = _addressConverter.OffsetToRva(Offset);
            }

            var result = new StringBuilder();

            result.Append("#define ").Append(Text).Append("_Offset ").AppendFormat("\"0x{0:X}\"", Offset).AppendLine()
                  .Append("#define ").Append(Text).Append("_Data ").AppendPatchData(PatchData);

            return result.ToString();
        }
    }
}
