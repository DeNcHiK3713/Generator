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
    class ComparePatch : PatchLine
    {
        public Line CalledMethod { get; set; }
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);
            CalledMethod.FindOffset(scriptJson);

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

                ulong offset;
                Mode mode;
                switch (architecture)
                {
                    case Architecture.ARM:
                        PatchData = new byte[] { 0x01 };
                        offset = 0;
                        mode = Mode.ARM;
                        break;
                    case Architecture.ARM64:
                        PatchData = new byte[] { 0x08 };
                        offset = 2;
                        mode = Mode.LITTLE_ENDIAN;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                using (Engine keystone = new Engine(architecture, mode) { ThrowOnError = true })
                {
                    var bl = $"bl #{CalledMethod.Offset};";
                    do
                    {
                        var buff = keystone.Assemble(bl, (ulong)il2cpp.Position).Buffer;
                        readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                        if (buffer.SequenceEqual(buff))
                        {
                            Offset = (ulong)il2cpp.Position + offset;
                            break;
                        }
                    }
                    while (readed < count);
                }
            }
        }
    }
}
