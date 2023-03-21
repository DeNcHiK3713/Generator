using Il2CppDumper;
using Gee.External.Capstone;
using Keystone;
using System;
using System.IO;
using System.Linq;
using Gee.External.Capstone.Arm;
using Gee.External.Capstone.Arm64;

namespace Generator.OffsetLines
{
    class AlwaysBranchComparePatch : PatchLine
    {
        public string Value { get; set; }
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);

            if (startOffset != 0)
            {
                ulong count = endOffset == 0 ? (ulong)il2cpp.Length - startOffset : endOffset - startOffset;
                il2cpp.Position = (long)startOffset;

                const byte bufferSize = 4;
                byte[] buffer = new byte[bufferSize];
                ulong readed = 0;

                Mode mode;
                switch (architecture)
                {
                    case Architecture.ARM:
                        mode = Mode.ARM;
                        break;
                    case Architecture.ARM64:
                        mode = Mode.LITTLE_ENDIAN;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                using (Engine keystone = new Engine(architecture, mode) { ThrowOnError = true })
                {
                    switch (architecture)
                    {
                        case Architecture.ARM:
                            using (var disassembler = CapstoneDisassembler.CreateArmDisassembler(ArmDisassembleMode.Arm))
                            {
                                while (readed < count)
                                {
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler.Disassemble(buffer).First();
                                    if (instruction.Id == ArmInstructionId.ARM_INS_CMP && instruction.Operand.EndsWith($" {Value}"))
                                    {
                                        Offset = (ulong)il2cpp.Position;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction = disassembler.Disassemble(buffer, (long)Offset).First();
                                        PatchData = keystone.Assemble($"b {instruction.Operand}", Offset).Buffer;
                                        break;
                                    }
                                }
                            }
                            break;
                        case Architecture.ARM64:
                            using (var disassembler2 = CapstoneDisassembler.CreateArm64Disassembler(Arm64DisassembleMode.LittleEndian))
                            {
                                while (readed < count)
                                {
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction2 = disassembler2.Disassemble(buffer).First();
                                    if (instruction2.Id == Arm64InstructionId.ARM64_INS_CMP && instruction2.Operand.EndsWith($" {Value}"))
                                    {
                                        Offset = (ulong)il2cpp.Position;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, (long)Offset).First();
                                        PatchData = keystone.Assemble($"b {instruction2.Operand}", Offset).Buffer;
                                        break;
                                    }
                                }
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
