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
    class ReplaceMethodPatch : PatchLine
    {
        public Line Search { get; set; }
        public Line Replace { get; set; }
        public Line GetThunkFrom { get; set; }

        protected ulong getThunkFromStartOffset;
        protected ulong getThunkFromEndOffset;

        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);
            Search.FindOffset(scriptJson);
            Replace.FindOffset(scriptJson);

            var index = GetThunkFrom.FindOffset(scriptJson);
            if (index != -1)
            {
                getThunkFromStartOffset = GetThunkFrom.Offset;
                switch (GetThunkFrom.Section)
                {
                    case ScriptSection.ScriptMethod:
                        if (index + 1 < scriptJson.ScriptMethod.Count)
                        {
                            getThunkFromEndOffset = scriptJson.ScriptMethod.ElementAt(index + 1).Address - 4;
                        }
                        break;
                    case ScriptSection.ScriptMetadata:
                        if (index + 1 < scriptJson.ScriptMetadata.Count)
                        {
                            getThunkFromEndOffset = scriptJson.ScriptMetadata.ElementAt(index + 1).Address - 4;
                        }
                        break;
                }
            }

            if (startOffset != 0)
            {
                ulong count = endOffset == 0 ? (ulong)il2cpp.Length - startOffset : endOffset - startOffset;

                ulong getThunkFromCount = getThunkFromEndOffset == 0 ? (ulong)il2cpp.Length - getThunkFromStartOffset : getThunkFromEndOffset - getThunkFromStartOffset;

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
                                        if (newPos == (long)Search.Offset)
                                        {
                                            Offset = (ulong)pos;
                                            PatchData = keystone.Assemble($"bl #{Replace.Offset};", Offset).Buffer;
                                            break;
                                        }
                                        var retPos = il2cpp.Position;
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
                                                if ((il2cpp.Position + BitConverter.ToInt32(buffer, 0)) == (long)Search.Offset)
                                                {
                                                    Offset = (ulong)pos;
                                                    break;
                                                }
                                            }
                                        }
                                        il2cpp.Position = retPos;

                                    }
                                }
                                while (readed < count);
                                if (Offset == 0 || PatchData != null)
                                {
                                    break;
                                }
                                readed = 0;
                                il2cpp.Position = (long)getThunkFromStartOffset;
                                do
                                {
                                    var pos = il2cpp.Position;
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler.Disassemble(buffer, pos).First();
                                    if (instruction.Id == ArmInstructionId.ARM_INS_BL)
                                    {
                                        var newPos = instruction.Details.Operands.First().Immediate;
                                        if (newPos == (long)Replace.Offset)
                                        {
                                            PatchData = keystone.Assemble($"bl #{Replace.Offset};", Offset).Buffer;
                                            break;
                                        }
                                        var retPos = il2cpp.Position;
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
                                                if ((il2cpp.Position + BitConverter.ToInt32(buffer, 0)) == (long)Replace.Offset)
                                                {
                                                    PatchData = keystone.Assemble($"bl #{newPos};", Offset).Buffer;
                                                    break;
                                                }
                                            }
                                        }
                                        il2cpp.Position = retPos;

                                    }
                                }
                                while (readed < getThunkFromCount);
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
                                        if (newPos == (long)Search.Offset)
                                        {
                                            Offset = (ulong)pos;
                                            PatchData = keystone.Assemble($"bl #{Replace.Offset};", Offset).Buffer;
                                            break;
                                        }
                                        var retPos = il2cpp.Position;
                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, newPos).First();
                                        if (instruction2.Id == Arm64InstructionId.ARM64_INS_B && instruction2.Details.Operands.First().Immediate == (long)Search.Offset)
                                        {
                                            Offset = (ulong)pos;
                                            break;
                                        }
                                        il2cpp.Position = retPos;
                                    }
                                }
                                while (readed < count);
                                if (Offset == 0 || PatchData != null)
                                {
                                    break;
                                }
                                readed = 0;
                                il2cpp.Position = (long)getThunkFromStartOffset;
                                do
                                {
                                    var pos = il2cpp.Position;
                                    readed += (ulong)il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction2 = disassembler2.Disassemble(buffer, pos).First();
                                    if (instruction2.Id == Arm64InstructionId.ARM64_INS_BL)
                                    {
                                        var newPos = instruction2.Details.Operands.First().Immediate;
                                        if (newPos == (long)Search.Offset)
                                        {
                                            PatchData = keystone.Assemble($"bl #{Replace.Offset};", Offset).Buffer;
                                            break;
                                        }
                                        var retPos = il2cpp.Position;
                                        il2cpp.Position = newPos;
                                        il2cpp.Read(buffer, 0, bufferSize);
                                        instruction2 = disassembler2.Disassemble(buffer, newPos).First();
                                        if (instruction2.Id == Arm64InstructionId.ARM64_INS_B && instruction2.Details.Operands.First().Immediate == (long)Search.Offset)
                                        {
                                            PatchData = keystone.Assemble($"bl #{newPos};", Offset).Buffer;
                                            break;
                                        }
                                        il2cpp.Position = retPos;
                                    }
                                }
                                while (readed < getThunkFromCount);
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
