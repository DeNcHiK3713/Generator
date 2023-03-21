using Generator;
using Generator.OffsetLines;
using Il2CppDumper;
using Keystone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

if (args.Length < 4 || !File.Exists(args[0]) || !File.Exists(args[1]) || !File.Exists(args[2]) || !Enum.TryParse<Architecture>(args[3], out var arch))
{
    Console.WriteLine("Usage: generator template.json script.json libil2cpp.so arch");
    return;
}
var lines = JsonConvert.DeserializeObject<IEnumerable<ILine>>(File.ReadAllText(args[0]), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
var scriptJson = JsonConvert.DeserializeObject<ScriptJson>(File.ReadAllText(args[1]));

using (var il2cpp = File.OpenRead(args[2]))
{
    lines.OfType<PatchLine>().ForEach(x => x.FindPatch(scriptJson, il2cpp, arch));
}

lines.ForEach(x => Console.WriteLine(x.GetLine(scriptJson)));
