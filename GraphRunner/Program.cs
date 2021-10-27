using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphRunner.Json;

namespace GraphRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new MyLogger();
            
            // デバッグ
            //Logger.LogLevel = Logger.LevelDebug;
            //Logger.SetLogMethod(msg => PrintText(msg));
            
            if (args.Length < 1)
            {
                logger.WriteLine("Usage : GraphRunner [run/interactive]");
                return;
            }

            if (args[0] == "run")
            {
                if (args.Length < 2)
                {
                    logger.WriteLine("Usage : GraphRunner run :filePath");
                    return;
                }

                await Execute(args[1],true);
            }
            else if (args[0] == "i" || args[0] == "interactive")
            {
                await StartInteractive(logger, args);
            }
            else
            {
                logger.WriteLine("Usage : GraphRunner [run/interactive]");
            }
        }

        static async Task<ExecutionEnv> Execute(string filePath,bool waitForServer)
        {
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
                await env.Execute(setting,PrintText);
            }

            if (json.Server != null)
            {
                var result = env.StartServer(json.Server.Port);

                if (!result)
                {
                    Console.WriteLine("Failed to start server.");   
                }
                
                foreach (var setting in json.Server.Bindings)
                {
                    env.AddPath(setting);
                }

                if (waitForServer)
                {
                    while (true)
                    {
                        var input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input)) continue;
                        if (input == "quit")
                        {
                            break;
                        }
                    }
                }
            }

            return env;
        }

        private static async Task StartInteractive(ILogger logger,string[] mainArgs)
        {
            if (mainArgs.Length >= 2)
            {
                logger.WriteLine("Loading {0}", mainArgs[1]);
            }

            ExecutionEnv env = mainArgs.Length >= 2 ? await Execute(mainArgs[1],false) : new ExecutionEnv();

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
                                     "exec/e : start execution");
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
                        logger.WriteLine("Usage : creategraph id:Int GraphType setting(optional)");
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
                        logger.WriteLine("OK. Created graph.");
                    }
                    else
                    {
                        //TODO エラーの詳細
                        logger.WriteLine("Failed to create graph.");
                    }
                }
                else if (args[0] == "removegraph" || args[0] == "rg")
                {
                    if (args.Length != 2)
                    {
                        logger.WriteLine("Usage : removegraph id:Int");
                        continue;
                    }
                    
                    if (!int.TryParse(args[1], out int id))
                    {
                        logger.WriteLine("id : ParseError");
                        continue;
                    }

                    if (env.RemoveGraph(id))
                    {
                        logger.WriteLine("Ok. Removed graph.");
                    }
                    else
                    {
                        logger.WriteLine("Failed to remove graph.");
                    }
                }
                else if (args[0] == "connect" || args[0] == "c")
                {
                    Connect(true, logger, env, args);
                }
                else if (args[0] == "disconnect" || args[0] == "d")
                {
                    Connect(false, logger, env, args);
                }
                else if (args[0] == "exec" || args[0] == "e")
                {
                    if (args.Length != 2)
                    {
                        logger.WriteLine("Usage : exec id:Int");
                        continue;
                    }

                    if (!int.TryParse(args[1], out int id))
                    {
                        logger.WriteLine("id : ParseError");
                        continue;
                    }

                    var setting = new Execution();
                    setting.GraphId = id;

                    var result = await env.Execute(setting, async (msg) =>
                    {
                        await PrintText(msg);
                        return true;
                    });
                    
                    if (result)
                    {
                        logger.WriteLine($"OK. Executed graph {id}.");
                    }
                    else
                    {
                        logger.WriteLine("Execution failed. Something went wrong.");
                    }
                }
                else
                {
                    logger.WriteLine("Command not found.\nType help to show commands.");
                }
            }
        }

        public static void Connect(bool connect,ILogger logger, ExecutionEnv env,string[] args)
        {
            if (args.Length != 7)
            {
                logger.WriteLine($"Usage : {(connect ? "connect" : "disconnect")} graphId nodeType nodeIndex graph2Id nodeType nodeIndex\n" +
                                 "NodeTypes: \n" +
                                 "  0:InProcess\n" +
                                 "  1:OutProcess\n" +
                                 "  2:InItem\n" +
                                 "  3:OutItem");
                return;
            }


            if (!int.TryParse(args[1], out int id1) ||
                !int.TryParse(args[2], out int nodeType1) ||
                !int.TryParse(args[3], out int nodeIndex1) ||
                !int.TryParse(args[4], out int id2) ||
                !int.TryParse(args[5], out int nodeType2) ||
                !int.TryParse(args[6], out int nodeIndex2))
            {
                logger.WriteLine("int.ParseError");
                return;
            }

            var setting = new NodeConnection();
            setting.Connect = connect;
            setting.From = new NodeInfo();
            setting.To = new NodeInfo();

            setting.From.GraphId = id1;
            setting.From.Type = nodeType1;
            setting.From.Index = nodeIndex1;

            setting.To.GraphId = id2;
            setting.To.Type = nodeType2;
            setting.To.Index = nodeIndex2;

            //TODO エラーの詳細

            if (env.ConnectNode(setting))
            {
                logger.WriteLine($"Ok. {(connect ? "Connected" : "Disconnected")} graph.");
            }
            else
            {
                logger.WriteLine($"Failed to {(connect ? "connect" : "disconnect")} graph.");
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