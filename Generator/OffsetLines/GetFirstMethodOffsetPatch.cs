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
                                        var newPos = instruction.Details.Operands.First().Immediate;

                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction = disassembler.Disassemble(buffer, newPos).FirstOrDefault();
                                        if (instruction is null)
                                        {
                                            Offset = (ulong)newPos;
                                            break;
                                        }
                                        if (instruction.Id == ArmInstructionId.ARM_INS_LDR && instruction.Operand == "ip, [pc]")
                                        {
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler.Disassemble(buffer, newPos).FirstOrDefault();
                                            if (instruction is null)
                                            {
                                                Offset = (ulong)newPos;
                                                break;
                                            }
                                            if (instruction.Id == ArmInstructionId.ARM_INS_ADD && instruction.Operand == "pc, pc, ip")
                                            {
                                                il2cpp.Read(buffer, 0, bufferSize);
                                                Offset = (ulong)(il2cpp.Position + BitConverter.ToInt32(buffer, 0));
                                                break;
                                            }
                                        }
                                        if (instruction.Id == ArmInstructionId.ARM_INS_MOVW)
                                        {
                                            var strOffs1 = instruction.Operand[(instruction.Operand.IndexOf('#') + 1)..];
                                            var offs1 = Convert.ToInt32(strOffs1, 16);
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler.Disassemble(buffer, newPos).FirstOrDefault();
                                            if (instruction is null)
                                            {
                                                Offset = (ulong)newPos;
                                                break;
                                            }
                                            if (instruction.Id == ArmInstructionId.ARM_INS_MOVT)
                                            {
                                                var strOffs2 = instruction.Operand[(instruction.Operand.IndexOf('#') + 1)..];
                                                var offs2 = Convert.ToInt32(strOffs2, 16);
                                                offs2 = ((offs2 << 16) | offs1) + newPos + 16;
                                                Offset = (ulong)offs2;
                                                break;
                                            }
                                        }
                                        if (instruction.Id == ArmInstructionId.ARM_INS_B)
                                        {
                                            Offset = (ulong)instruction.Details.Operands.First().Immediate;
                                            break;
                                        }
                                        Offset = (ulong)newPos;
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
                                        var newPos = instruction2.Details.Operands.First().Immediate;

                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, newPos).FirstOrDefault();
                                        if (instruction2 is null)
                                        {
                                            Offset = (ulong)newPos;
                                            break;
                                        }
                                        if (instruction2.Id == Arm64InstructionId.ARM64_INS_B)
                                        {
                                            Offset = (ulong)instruction2.Details.Operands.First().Immediate;
                                            break;
                                        }
                                        Offset = (ulong)newPos;
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
