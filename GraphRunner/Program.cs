using System;
using System.IO;
using System.Text.Json;

namespace GraphRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            
            if (args.Length != 1)
            {
                Console.WriteLine("Usage : cmd [FilePath]");
                return;
            }
            
            string jsonString = File.ReadAllText(args[0]);
            
            var setting = JsonSerializer.Deserialize<ExecutionSetting>(jsonString);

            if (setting == null)
            {
                Console.WriteLine("ParseError");
                return;
            }
            
            Console.WriteLine(setting.Graphs == null);
        }
    }
}