using Gee.External.Capstone;
using Gee.External.Capstone.Arm;
using Gee.External.Capstone.Arm64;
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
    class GetFirstMethodOffsetPatch : PatchLine
    {
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture, Il2CppAddressConverter addressConverter)
        {
            base.FindPatch(scriptJson, il2cpp, architecture, addressConverter);

            if (startOffset != 0)
            {
                ulong count = endOffset == 0 ? (ulong)il2cpp.Length - startOffset : endOffset - startOffset;

                il2cpp.Position = (long)startOffset;

                const byte bufferSize = 4;
                byte[] buffer = new byte[bufferSize];
                ulong readed = 0;

                Mode mode = architecture == Architecture.ARM ? Mode.ARM : Mode.LITTLE_ENDIAN;

                using (Engine keystone = new Engine(architecture, mode) { ThrowOnError = true })
                {
                    switch (architecture)
                    {
                        case Architecture.ARM:
                            using (var disassembler = CapstoneDisassembler.CreateArmDisassembler(ArmDisassembleMode.Arm))
                            {
                                disassembler.EnableInstructionDetails = true;
                                do
                                {
                                    var pos = il2cpp.Position;
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler.Disassemble(buffer, pos).FirstOrDefault();
                                    if (instruction is null)
                                    {
                                        continue;
                                    }
                                    if (instruction.Id == ArmInstructionId.ARM_INS_BL)
                                    {
                                        Offset = (ulong)instruction.Details.Operands.First().Immediate;
                                        break;
                                    }
                                }
                                while (readed < count);
                            }
                            break;
                        case Architecture.ARM64:
                            using (var disassembler2 = CapstoneDisassembler.CreateArm64Disassembler(Arm64DisassembleMode.LittleEndian))
                            {
                                disassembler2.EnableInstructionDetails = true;
                                do
                                {
                                    var pos = il2cpp.Position;
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction2 = disassembler2.Disassemble(buffer, pos).FirstOrDefault();
                                    if (instruction2 is null)
                                    {
                                        continue;
                                    }
                                    if (instruction2.Id == Arm64InstructionId.ARM64_INS_BL)
                                    {
                                        Offset = (ulong)instruction2.Details.Operands.First().Immediate;
                                        break;
                                    }
                                }
                                while (readed < count);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
