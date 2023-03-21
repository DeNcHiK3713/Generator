using Gee.External.Capstone;
using Gee.External.Capstone.Arm;
using Gee.External.Capstone.Arm64;
using Il2CppDumper;
using Keystone;
using System;
using System.IO;
using System.Linq;

namespace Generator.OffsetLines
{
    class ReplaceMethodArgumentPatch : PatchLine
    {
        public Line CalledMethod { get; set; }
        public int Argument { get; set; }
        public string Value { get; set; }
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
                                    var instruction = disassembler.Disassemble(buffer, pos).First();
                                    if (instruction.Id == ArmInstructionId.ARM_INS_BL)
                                    {
                                        var newPos = instruction.Details.Operands.First().Immediate;
                                        if (newPos == (long)CalledMethod.Offset)
                                        {
                                            do
                                            {
                                                il2cpp.Position -= 8;
                                                readed -= (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                                instruction = disassembler.Disassemble(buffer).First();
                                                if (instruction.Id == ArmInstructionId.ARM_INS_MOV && instruction.Operand.StartsWith($"r{Argument},"))
                                                {
                                                    Offset = (ulong)il2cpp.Position - 4;
                                                    PatchData = keystone.Assemble($"mov r{Argument}, {Value}", Offset).Buffer;
                                                    break;
                                                }
                                            }
                                            while (readed > 0);
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
                                                    il2cpp.Position = pos;
                                                    do
                                                    {
                                                        il2cpp.Position -= 8;
                                                        readed -= (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                                        instruction = disassembler.Disassemble(buffer).First();
                                                        if (instruction.Id == ArmInstructionId.ARM_INS_MOV && instruction.Operand.StartsWith($"r{Argument},"))
                                                        {
                                                            Offset = (ulong)il2cpp.Position - 4;
                                                            PatchData = keystone.Assemble($"mov r{Argument}, {Value}", Offset).Buffer;
                                                            break;
                                                        }
                                                    }
                                                    while (readed > 0);
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
                                            do
                                            {
                                                il2cpp.Position -= 8;
                                                readed -= (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                                instruction2 = disassembler2.Disassemble(buffer).First();
                                                if (instruction2.Id == Arm64InstructionId.ARM64_INS_MOV && instruction2.Operand.StartsWith($"w{Argument},"))
                                                {
                                                    Offset = (ulong)il2cpp.Position - 4;
                                                    PatchData = keystone.Assemble($"mov w{Argument}, {Value}", Offset).Buffer;
                                                    break;
                                                }
                                            }
                                            while (readed > 0);
                                            break;
                                        }
                                        pos = il2cpp.Position;
                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, newPos).First();
                                        if (instruction2.Id == Arm64InstructionId.ARM64_INS_B && instruction2.Details.Operands.First().Immediate == (long)CalledMethod.Offset)
                                        {
                                            il2cpp.Position = pos;
                                            do
                                            {
                                                il2cpp.Position -= 8;
                                                readed -= (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                                instruction2 = disassembler2.Disassemble(buffer).First();
                                                if (instruction2.Id == Arm64InstructionId.ARM64_INS_MOV && instruction2.Operand.StartsWith($"w{Argument},"))
                                                {
                                                    Offset = (ulong)il2cpp.Position - 4;
                                                    PatchData = keystone.Assemble($"mov w{Argument}, {Value}", Offset).Buffer;
                                                    break;
                                                }
                                            }
                                            while (readed > 0);
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
