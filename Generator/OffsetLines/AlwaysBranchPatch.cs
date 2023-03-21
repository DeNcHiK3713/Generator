using Il2CppDumper;
using Gee.External.Capstone;
using Keystone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gee.External.Capstone.Arm;
using Gee.External.Capstone.Arm64;

namespace Generator.OffsetLines
{
    class AlwaysBranchPatch : PatchLine
    {
        public Line CalledMethod { get; set; }
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);
            CalledMethod.FindOffset(scriptJson);

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
                                disassembler.EnableInstructionDetails = true;
                                do
                                {
                                    var pos = il2cpp.Position;
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler.Disassemble(buffer, pos).First();
                                    if (instruction.Id == ArmInstructionId.ARM_INS_BL)
                                    {
                                        var newPos = instruction.Details.Operands.First().Immediate;
                                        if (newPos == (long)CalledMethod.Offset)
                                        {
                                            il2cpp.Position += 4;
                                            Offset = (ulong)il2cpp.Position;
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler.Disassemble(buffer, (long)Offset).First();
                                            PatchData = keystone.Assemble($"b {instruction.Operand}", Offset).Buffer;
                                            break;
                                        }
                                        pos = il2cpp.Position;
                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction = disassembler.Disassemble(buffer, newPos).First();
                                        if (instruction.Id == ArmInstructionId.ARM_INS_LDR && instruction.Operand == "ip, [pc]")
                                        {
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler.Disassemble(buffer, newPos).First();
                                            if (instruction.Id == ArmInstructionId.ARM_INS_ADD && instruction.Operand == "pc, pc, ip")
                                            {
                                                il2cpp.Read(buffer, 0, bufferSize);
                                                if (il2cpp.Position + BitConverter.ToInt32(buffer, 0) == (long)CalledMethod.Offset)
                                                {
                                                    il2cpp.Position = pos + 4;
                                                    Offset = (ulong)il2cpp.Position;
                                                    il2cpp.Read(buffer, 0, bufferSize);
                                                    instruction = disassembler.Disassemble(buffer, (long)Offset).First();
                                                    PatchData = keystone.Assemble($"b {instruction.Operand}", Offset).Buffer;
                                                    break;
                                                }
                                            }
                                        }
                                        il2cpp.Position = pos;

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
                                    var instruction2 = disassembler2.Disassemble(buffer, pos).First();
                                    if (instruction2.Id == Arm64InstructionId.ARM64_INS_BL)
                                    {
                                        var newPos = instruction2.Details.Operands.First().Immediate;
                                        if (newPos == (long)CalledMethod.Offset)
                                        {
                                            Offset = (ulong)il2cpp.Position;
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction2 = disassembler2.Disassemble(buffer, (long)Offset).First();
                                            PatchData = keystone.Assemble($"b {instruction2.Operand[instruction2.Operand.LastIndexOf('#')..]}", Offset).Buffer;
                                            break;
                                        }
                                        pos = il2cpp.Position;
                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, newPos).First();
                                        if (instruction2.Id == Arm64InstructionId.ARM64_INS_B && instruction2.Details.Operands.First().Immediate == (long)CalledMethod.Offset)
                                        {
                                            il2cpp.Position = pos;
                                            Offset = (ulong)pos;
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction2 = disassembler2.Disassemble(buffer, (long)Offset).First();
                                            PatchData = keystone.Assemble($"b {instruction2.Operand[instruction2.Operand.LastIndexOf('#')..]}", Offset).Buffer;
                                            break;
                                        }
                                        il2cpp.Position = pos;
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
