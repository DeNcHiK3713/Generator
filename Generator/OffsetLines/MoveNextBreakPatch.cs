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
    class MoveNextBreakPatch : PatchLine
    {
        public Line CalledMethod { get; set; }
        public override void FindPatch(ScriptJson scriptJson, Stream il2cpp, Architecture architecture)
        {
            base.FindPatch(scriptJson, il2cpp, architecture);
            CalledMethod?.FindOffset(scriptJson);

            if (startOffset != 0)
            {
                endOffset = endOffset == 0 ? (ulong)il2cpp.Length : endOffset;
                il2cpp.Position = (long)startOffset;

                const byte bufferSize = 4;
                byte[] buffer = new byte[bufferSize];

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

                                Func<ArmInstruction, bool> query = CalledMethod is null ?
                                    (instruction) => instruction.Id == ArmInstructionId.ARM_INS_MVN && instruction.Operand == "r0, #0" :
                                    (instruction) => instruction.Id == ArmInstructionId.ARM_INS_BL && (ulong)instruction.Details.Operands.FirstOrDefault()?.Immediate == CalledMethod.Offset;

                                var patchData = CalledMethod is null ?
                                    "str r0, [r4, #8]; mov r0, #0; b #{0}":
                                    "mvn r0, #0; str r0, [r4, #8]; mov r0, #0; b #{0}";

                                while (il2cpp.Position < (long)endOffset)
                                {
                                    var pos = il2cpp.Position;
                                    il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler.Disassemble(buffer, pos).FirstOrDefault();
                                    if (instruction is not null && query(instruction))
                                    {
                                        il2cpp.Position = (long)endOffset - 4;
                                        while (il2cpp.Position > (long)startOffset)
                                        {
                                            var pos2 = il2cpp.Position;
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler.Disassemble(buffer, pos2).FirstOrDefault();
                                            if (instruction is not null && instruction.Details.AllWrittenRegisters.Any(x => x.Id == ArmRegisterId.ARM_REG_R0))
                                            {
                                                Offset = (ulong)pos + 4;
                                                PatchData = keystone.Assemble(string.Format(patchData, pos2), Offset).Buffer;
                                                break;
                                            }
                                            il2cpp.Position -= 8;
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        case Architecture.ARM64:
                            using (var disassembler2 = CapstoneDisassembler.CreateArm64Disassembler(Arm64DisassembleMode.LittleEndian))
                            {
                                disassembler2.EnableInstructionDetails = true;

                                Func<Arm64Instruction, bool> query = CalledMethod is null ?
                                    (instruction) => instruction.Id == Arm64InstructionId.ARM64_INS_MOVN && instruction.Operand == "w8, #0" :
                                    (instruction) => instruction.Id == Arm64InstructionId.ARM64_INS_BL && (ulong)instruction.Details.Operands.FirstOrDefault()?.Immediate == CalledMethod.Offset;

                                Action<Engine, ulong> patch = CalledMethod is null ?
                                    (keystone, end) => PatchData = keystone.Assemble($"str w8, [x19, #0x10]; mov w0, wzr; b #{end}", Offset).Buffer :
                                    (keystone, end) => PatchData = keystone.Assemble($"movn w8, #0; str w8, [x19, #0x10]; mov w0, wzr; b #{end}", Offset).Buffer;

                                while (il2cpp.Position < (long)endOffset)
                                {
                                    var pos = il2cpp.Position;
                                    il2cpp.Read(buffer, 0, bufferSize);
                                    var instruction = disassembler2.Disassemble(buffer, pos).FirstOrDefault();
                                    if (instruction is not null && query(instruction))
                                    {
                                        il2cpp.Position = (long)endOffset - 4;
                                        while (il2cpp.Position > (long)startOffset)
                                        {
                                            var pos2 = il2cpp.Position;
                                            il2cpp.Read(buffer, 0, bufferSize);
                                            instruction = disassembler2.Disassemble(buffer, pos2).FirstOrDefault();
                                            if (instruction is not null && instruction.Details.AllWrittenRegisters.Any(x => x.Id == Arm64RegisterId.ARM64_REG_W0))
                                            {
                                                Offset = (ulong)pos + 4;
                                                patch(keystone, (ulong)il2cpp.Position);
                                                break;
                                            }
                                            il2cpp.Position -= 8;
                                        }
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
