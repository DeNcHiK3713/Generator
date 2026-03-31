using ELFSharp.ELF;
using Generator;
using Generator.OffsetLines;
using Il2CppDumper;
using Keystone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

if (args.Length < 3 || !File.Exists(args[0]) || !File.Exists(args[1]) || !File.Exists(args[2]))
{
    Console.WriteLine("Usage: generator template.json script.json libil2cpp.so");
    return;
}
var lines = JsonConvert.DeserializeObject<IEnumerable<ILine>>(File.ReadAllText(args[0]), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
var scriptJson = JsonConvert.DeserializeObject<ScriptJson>(File.ReadAllText(args[1]));

using (var il2cpp = File.OpenRead(args[2]))
{
    var pos = il2cpp.Position;
    using var elf = ELFReader.Load(il2cpp, false);
    var machine = elf.Machine;
    il2cpp.Position = pos;

    var arch = machine switch {
        Machine.ARM => Architecture.ARM,
        Machine.AArch64 => Architecture.ARM64,
        _ => throw new NotSupportedException()
    };

    var addressConverter = new Il2CppAddressConverter(il2cpp, arch);
    lines.OfType<PatchLine>().ForEach(x => x.FindPatch(scriptJson, il2cpp, arch, addressConverter));
}

lines.ForEach(x => Console.WriteLine(x.GetLine(scriptJson)));
