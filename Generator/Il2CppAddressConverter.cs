using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Keystone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Generator
{
    public class Il2CppAddressConverter
    {
        private readonly ulong _loadAddress;
        private readonly ulong _size;
        private readonly ulong _offset;

        public Il2CppAddressConverter(Stream stream, Architecture arch)
        {
            var pos = stream.Position;
            using var elf = ELFReader.Load(stream, false);
            var section = elf.GetSection("il2cpp");

            switch (arch)
            {
                case Architecture.ARM:
                    var armSection = section as ProgBitsSection<uint>;
                    _loadAddress = armSection.LoadAddress;
                    _size = armSection.Size;
                    _offset = armSection.Offset;
                    break;
                case Architecture.ARM64:
                    var arm64Section = section as ProgBitsSection<ulong>;
                    _loadAddress = arm64Section.LoadAddress;
                    _size = arm64Section.Size;
                    _offset = arm64Section.Offset;
                    break;
            }

            stream.Position = pos;
        }

        public ulong RvaToOffset(ulong rva)
        {
            if (rva >= _loadAddress && rva < _loadAddress + _size)
            {
                return rva - _loadAddress + _offset;
            }

            return rva;
        }

        public ulong OffsetToRva(ulong offset)
        {
            if (offset >= _offset && offset < _offset + _size)
            {
                return offset - _offset + _loadAddress;
            }

            return offset;
        }
    }

}
