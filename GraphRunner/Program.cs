using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GraphRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (args.Length != 1)
            {
                Console.WriteLine("Usage : cmd [FilePath]");
                return;
            }

            Read(args[0]);
        }

        static void Read(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);

            var setting = JsonSerializer.Deserialize<ExecutionSetting>(jsonString);

            if (setting == null)
            {
                Console.WriteLine("ParseError");
                return;
            }

            Console.WriteLine(setting.Graphs[0].Type);
        }

        static void CreateTemplate()
        {
            var a = new ExecutionSetting();
            a.Graphs = new List<GraphSetting>();
            a.Connections = new List<NodeConnection>();
            a.Executions = new List<Execution>();

            a.Graphs.Add(new GraphSetting());
            a.Graphs[0].Id = "1";
            a.Graphs[0].Type = "Updater";
            a.Graphs[0].Setting = new Dictionary<string, string>();

            a.Graphs[0].Setting["Time"] = "1";

            Console.WriteLine(JsonSerializer.Serialize(a));
        }
    }
}