using ELFSharp.ELF;
using Generator;
using Generator.OffsetLines;
using Il2CppDumper;
using Keystone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;

var templateArgument = new Argument<FileInfo>("template.json")
{
    Description = "Path to template.json file"
};

var scriptArgument = new Argument<FileInfo>("script.json")
{
    Description = "Path to script.json file"
};

var libArgument = new Argument<FileInfo>("libil2cpp.so")
{
    Description = "Path to libil2cpp.so file"
};

var offsetOption = new Option<bool>("--no-relative", "-nr")
{
    Description = "Output addresses as file offsets (file-based patches), otherwise as relative offsets (memory-based patches). Default outputs as relative offsets",
    Required = false
};

var rootCommand = new RootCommand("Generate offsets from Il2CppDumper output");
rootCommand.Arguments.Add(templateArgument);
rootCommand.Arguments.Add(scriptArgument);
rootCommand.Arguments.Add(libArgument);
rootCommand.Options.Add(offsetOption);

rootCommand.SetAction(parseResult =>
{
    var templateFile = parseResult.GetValue(templateArgument);
    var scriptFile = parseResult.GetValue(scriptArgument);
    var libFile = parseResult.GetValue(libArgument);
    var relative = !parseResult.GetValue(offsetOption);

    var lines = JsonConvert.DeserializeObject<IEnumerable<ILine>>(File.ReadAllText(templateFile.FullName), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
    var scriptJson = JsonConvert.DeserializeObject<ScriptJson>(File.ReadAllText(scriptFile.FullName));

    using var il2cpp = libFile.OpenRead();
    var pos = il2cpp.Position;
    using var elf = ELFReader.Load(il2cpp, false);
    var machine = elf.Machine;
    il2cpp.Position = pos;

    var arch = machine switch
    {
        Machine.ARM => Architecture.ARM,
        Machine.AArch64 => Architecture.ARM64,
        _ => throw new NotSupportedException()
    };

    var addressConverter = new Il2CppAddressConverter(il2cpp, arch);
    lines.OfType<PatchLine>().ForEach(x => x.FindPatch(scriptJson, il2cpp, arch, addressConverter));

    lines.ForEach(x => Console.WriteLine(x.GetLine(scriptJson, relative, addressConverter)));
});

return rootCommand.Parse(args).Invoke();
