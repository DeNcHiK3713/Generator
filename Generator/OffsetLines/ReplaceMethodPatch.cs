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
    class ReplaceMethodPatch : PatchLine
    {
        public Line Search { get; set; }
        public Line Replace { get; set; }
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);
            Search.FindOffset(scriptJson);
            Replace.FindOffset(scriptJson);

            if (startOffset != 0)
            {
                ulong count;

                if (endOffset == 0)
                {
                    count = (ulong)il2cpp.Length - startOffset;
                }
                else
                {
                    count = endOffset - startOffset;
                }

                il2cpp.Position = (long)startOffset;

                const byte bufferSize = 4;
                byte[] buffer = new byte[bufferSize];
                ulong readed = 0;

                Mode mode = architecture == Architecture.ARM ? Mode.ARM : Mode.LITTLE_ENDIAN;

                using (Engine keystone = new Engine(architecture, mode) { ThrowOnError = true })
                {
                    var bl = $"bl #{Search.Offset};";
                    do
                    {
                        var buff = keystone.Assemble(bl, (ulong)il2cpp.Position).Buffer;
                        readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                        if (buffer.SequenceEqual(buff))
                        {
                            Offset = (ulong)il2cpp.Position - 4;
                            PatchData = keystone.Assemble($"bl #{Replace.Offset};", Offset).Buffer;
                            break;
                        }
                    }
                    while (readed < count);
                }
            }
        }
    }
}
