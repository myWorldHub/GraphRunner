using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphRunner.Json;

namespace GraphRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage : GraphRunner [FilePath]");
                return;
            }
            await Execute(args[0]);
        }

        static async Task Execute(string filePath)
        {
            var json = JsonSerializer.Deserialize<ExecutionSetting>(await File.ReadAllTextAsync(filePath));

            if (json == null)
            {
                Console.WriteLine("ParseError");
                return;
            }

            var env = new ExecutionEnv();

            foreach (var setting in json.Graphs)
            {
                if (!env.AddGraph(setting))
                {
                    throw new EvaluateException($"Failed to instantiate graph : id {setting.Id}.");
                }
            }

            foreach (var setting in json.Connections)
            {
                if (!env.ConnectNode(setting))
                {
                    throw new EvaluateException(
                        $"Failed to {(setting.Connect ? "dis" : "")}connect node : {setting.From} & {setting.To}.");
                }
            }

            foreach (var setting in json.Executions)
            {
                await env.Execute(setting);
            }

        }

        public static Task<bool> PrintText(string message)
        {
            Console.WriteLine(message);
            return Task.FromResult(true);
        }
    }
}