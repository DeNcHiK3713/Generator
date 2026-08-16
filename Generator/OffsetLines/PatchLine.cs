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
        public string PatchDataText { get; set; }

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

        public string GetLine(ScriptJson scriptJson, bool relative = true, Il2CppAddressConverter addressConverter = null)
        {
            if (relative && addressConverter is not null)
            {
                if (this is GetFirstMethodOffsetPatch)
                {
                    Offset = addressConverter.OffsetToRvaUnsafe(Offset);
                }
                else
                {
                    Offset = addressConverter.OffsetToRva(Offset);
                }
            }

            var result = new StringBuilder().AppendFormat(Text, Offset);

            if (PatchData is not null && PatchDataText is not null)
            {
                var patchDataHex = BitConverter.ToString(PatchData).Replace("-", " ");
                result.AppendLine()
                      .AppendFormat(PatchDataText, patchDataHex);
            }

            return result.ToString();
        }
    }
}
