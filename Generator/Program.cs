using Generator.OffsetLines;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || !File.Exists(args[0]) || !File.Exists(args[1]))
            {
                Console.WriteLine("Usage: generator template.json script.json");
                return;
            }
            var lines = JsonConvert.DeserializeObject<IEnumerable<IOffsetLine>>(File.ReadAllText(args[0]), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
#if DEBUG
            var l = File.ReadAllLines(args[1]);
            lines = lines.OrderBy(x => x.FindIndex(l));
            File.WriteAllText(args[0], JsonConvert.SerializeObject(lines, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented }));
#else
            using (var reader = new StreamReader(args[1]))
            {
                lines.ForEach(x => Console.WriteLine(x.GetLine(reader)));
            }
#endif
        }
    }
}
