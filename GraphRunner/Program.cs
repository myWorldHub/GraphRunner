using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphConnectEngine;

namespace GraphRunner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new MyLogger();
         
            if (args.Length < 1)
            {
                logger.WriteLine("Usage : GraphRunner [run/interactive]");
                return;
            }

            for(int i=0;i<args.Length;i++)
            {
                if(args[i] == "-l")
                {
                    Logger.LogLevel = Logger.LevelDebug;
                    Logger.SetLogMethod(msg => PrintText(msg));
                    string[] na = new string[args.Length - 1];
                    for(int j = 0; j < i; j++) na[j] = args[j];
                    for (int j = i + 1; j < na.Length; j++) na[j - 1] = args[j];
                    args = na;
                    break;
                }
            }

            if (args[0] == "run")
            {
                logger.WriteLine("Bye");
                /*
                if (args.Length < 2)
                {
                    logger.WriteLine("Usage : GraphRunner run :filePath");
                    return;
                }
                await Execute();
                */
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

        private static ExecutionSetting GetExecutionSettingFromJson(string filePath)
        {
            string fileStr;
            try
            {
                fileStr = File.ReadAllText(filePath);
                var setting = JsonSerializer.Deserialize<ExecutionSetting>(fileStr);
                return setting;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static async Task StartInteractive(ILogger logger,string[] mainArgs)
        {
            ExecutionEnv env;
            if (mainArgs.Length >= 2)
            {
                logger.WriteLine("Loading {0}", mainArgs[1]);
                var setting = GetExecutionSettingFromJson(mainArgs[1]);
                if(setting == null)
                {
                    env = new ExecutionEnv();
                    logger.WriteLine("Failed to load {0}", mainArgs[1]);
                }
                else
                {
                    env = ExecutionEnv.FromSetting(setting);
                    logger.WriteLine("Loaded {0}", mainArgs[1]);
                }
            }
            else
            {
                env = new ExecutionEnv();
            }

            while (true)
            {
                logger.Write(">");
                
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                
                var args = input.Split(" ");

                if (args[0] == "h" || args[0] == "help")
                {
                    logger.WriteLine("help/h : show commands\n" +
                                     "quit/q : quit interactive mode\n" +
                                     "creategraph/cg : create graph\n" +
                                     "removegraph/rg : remove graph\n" +
                                     "connect/c : connect node\n" +
                                     "disconnect/d : disconnect node\n" +
                                     "exec/e : start execution\n" +
                                     "server/s : server command");
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

                    var result = env.AddGraph(id,setting);
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

                    var result = await env.Execute(id, async (msg) =>
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
                else if (args[0] == "server" || args[0] == "s")
                {
                    if (args.Length == 1 || args[1] == "" || args[1] == "help")
                    {
                        logger.WriteLine("Usage : server [help/start/stop/bind/remove]");
                        continue;
                    }

                    if (args[1] == "start")
                    {
                        if (args.Length != 3)
                        {
                            logger.WriteLine("Usage : server start [port]");
                            continue;
                        }

                        if (!int.TryParse(args[2], out var port))
                        {
                            logger.WriteLine("port : ParseError");
                            continue;
                        }
                        
                        if (env.IsServerRunning)
                        {
                            logger.WriteLine($"Server is already running on port {env.Port}");
                            continue;
                        }

                        env.StartServer(port);
                    }
                    else if (args[1] == "stop")
                    {
                        if (!env.IsServerRunning)
                        {
                            logger.WriteLine("Server is not running.");
                            continue;
                        }

                        if(env.StopServer())
                        {
                            logger.WriteLine("Successfully stopped server.");
                        }
                        else
                        {
                            logger.WriteLine("Failed to stop server.");
                        }
                    }
                    else if (args[1] == "bind")
                    {
                        if (args.Length != 4)
                        {
                            logger.WriteLine("Usage : server bind [path] [updaterGraphId]");
                            continue;
                        }

                        var path = args[2];
                        
                        if (!int.TryParse(args[3], out var updaterId))
                        {
                            logger.WriteLine("updaterId : ParseError");
                            continue;
                        }

                        if (env.AddPath(path,updaterId))
                        {
                            logger.WriteLine($"OK. {path} -> Graph[{updaterId}]");
                        }
                        else
                        {
                            logger.WriteLine("Failed to bind path.");
                        }
                    }
                    else if (args[1] == "remove")
                    {
                        if (args.Length != 3)
                        {
                            logger.WriteLine("Usage : server remove [path]");
                            continue;
                        }

                        var path = args[2];

                        if (env.RemovePath(path))
                        {
                            logger.WriteLine($"OK. Removed {path}");
                        }
                        else
                        {
                            logger.WriteLine("Failed to remove path.");
                        }
                    }
                    else
                    {
                        logger.WriteLine("Usage : server [help/start/stop/bind/remove]");
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


            if (!int.TryParse(args[1], out int graphId1) ||
                !int.TryParse(args[2], out int typeInt1) ||
                !int.TryParse(args[3], out int index1) ||
                !int.TryParse(args[4], out int graphId2) ||
                !int.TryParse(args[5], out int typeInt2) ||
                !int.TryParse(args[6], out int index2))
            {
                logger.WriteLine("int.ParseError");
                return;
            }

            var type1 = NodeData.IntToNodeType(typeInt1);
            var type2 = NodeData.IntToNodeType(typeInt2);

            if (type1 == NodeType.Undefined || type2 == NodeType.Undefined)
            {
                logger.WriteLine("NodeType Error.");
                return;
            }

            //TODO エラーの詳細

            if (env.ConnectNode(new NodeData
            {
                GraphId = graphId1,
                NodeType = type1,
                Index = index1
            }, new NodeData
            {
                GraphId = graphId2,
                NodeType = type2,
                Index = index2
            }))
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