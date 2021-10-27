using System;
using System.Collections.Generic;
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
            ILogger logger = new MyLogger();
            
            if (args.Length < 1)
            {
                logger.WriteLine("Usage : GraphRunner [run/i]");
                return;
            }

            switch (args[0])
            {
                case "run":
                    if (args.Length < 2)
                    {
                        logger.WriteLine("Usage : GraphRunner run :filePath");
                        return;
                    }

                    await Execute(args[1]);
                    break;
                case "i":
                    await StartInteractive(logger,args);
                    break;
                default:
                    Console.WriteLine("Usage : GraphRunner [run/i]");
                    break;
            }
            
            
        }

        static async Task<ExecutionEnv> Execute(string filePath)
        {
            //TODO サーバーモード
            
            var json = JsonSerializer.Deserialize<ExecutionSetting>(await File.ReadAllTextAsync(filePath));

            if (json == null)
            {
                throw new Exception("Failed to parse json.");
            }

            var env = new ExecutionEnv();

            foreach (var setting in json.Graphs)
            {
                if (!env.AddGraph(setting))
                {
                    throw new Exception($"Failed to instantiate graph : id {setting.Id}.");
                }
            }

            foreach (var setting in json.Connections)
            {
                if (!env.ConnectNode(setting))
                {
                    throw new Exception(
                        $"Failed to {(setting.Connect ? "dis" : "")}connect node : {setting.From} & {setting.To}.");
                }
            }

            foreach (var setting in json.Executions)
            {
                await env.Execute(setting);
            }

            return env;
        }

        private static async Task StartInteractive(ILogger logger,string[] mainArgs)
        {
            //TODO　argsでサーバーのポート指定
            
            if (mainArgs.Length >= 2)
            {
                logger.WriteLine("Loading {0}", mainArgs[1]);
            }

            ExecutionEnv env = mainArgs.Length >= 2 ? await Execute(mainArgs[1]) : new ExecutionEnv();

            while (true)
            {
                logger.Write(">");
                
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                
                var args = input.Split();

                if (args[0] == "h" || args[0] == "help")
                {
                    logger.WriteLine("help/h : show commands\n" +
                                     "quit/q : quit interactive mode\n" +
                                     "creategraph/cg : create graph\n" +
                                     "removegraph/rg : remove graph\n" +
                                     "connect/c : connect node\n" +
                                     "disconnect/d : disconnect node\n" +
                                     "exec/e : start execution\n" +
                                     "bind/b : bind graph execution to server");
                }
                else if (args[0] == "q" || args[0] == "quit")
                {
                    logger.WriteLine("Bye");
                    break;
                }
                else if (args[0] == "creategraph" || args[0] == "cg")
                {
                    if (args.Length < 3)
                    {
                        logger.WriteLine("creategraph id:Int GraphType setting(optional)");
                        continue;
                    }

                    if (!int.TryParse(args[1], out int id))
                    {
                        logger.WriteLine("id : ParseError");
                        continue;
                    }

                    GraphSetting setting = new GraphSetting();
                    setting.Id = id;
                    setting.Type = args[2];
                    setting.Setting = new Dictionary<string, string>();

                    for (int i = 3; i < args.Length; i++)
                    {
                        var data = args[i].Split(":");
                        if (data.Length == 2)
                        {
                            setting.Setting[data[0]] = data[1];
                        }
                        else
                        {
                            logger.WriteLine("setting : ParseError");
                        }
                    }

                    var result = env.AddGraph(setting);
                    if (result)
                    {
                        logger.WriteLine("OK. Created graph");
                    }
                    else
                    {
                        //TODO エラーの詳細
                        logger.WriteLine("Failed to create graph.");
                    }
                }
                else if (args[0] == "removegraph" || args[0] == "rg")
                {
                    //TODO
                }
                else if (args[0] == "connect" || args[0] == "c")
                {
                    //TODO
                }
                else if (args[0] == "disconnect" || args[0] == "d")
                {
                    //TODO
                }
                else if (args[0] == "exec" || args[0] == "e")
                {
                    //TODO
                }
                else if (args[0] == "bind" || args[0] == "b")
                {
                    //TODO
                }
                else
                {
                    logger.WriteLine("Command not found.\nType help to show commands.");
                }
            }
        }

        public static Task<bool> PrintText(string message)
        {
            //TODO
            Console.WriteLine(message);
            return Task.FromResult(true);
        }
    }
}