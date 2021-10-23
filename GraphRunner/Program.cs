using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Graphs;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;

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

            var sw = new System.Diagnostics.Stopwatch();
            
            sw.Start();
            Execute(args[0]);
            sw.Stop();
            
            // 結果表示
            Console.WriteLine("Time");
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"　{ts}");
            Console.WriteLine($"　{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
            Console.WriteLine($"　{sw.ElapsedMilliseconds}ミリ秒");
        }

        static async void Execute(string filePath)
        {
            var json = JsonSerializer.Deserialize<ExecutionSetting>(File.ReadAllText(filePath));

            if (json == null)
            {
                Console.WriteLine("ParseError");
                return;
            }

            var graphs = new Dictionary<int, IGraph>();
            NodeConnector connector = new NodeConnector();

            foreach (var setting in json.Graphs)
            {
                IGraph graph;
                switch (setting.Type)
                {
                    case "Updater":
                        graph = new UpdaterGraph(connector,new SerialSender());
                        break;
                    case "PrintText":
                        graph = new DebugTextGraph(connector, PrintText);
                        break;
                    case "Value":
                        switch (setting.Setting["Type"])
                        {
                            case "Int":
                                graph = new ValueGraph<int>(connector, int.Parse(setting.Setting["Value"]));
                                break;
                            default:
                                throw new ArgumentException($"ValueType <{setting.Type}> is not found.");
                        }
                        break;
                    default:
                        throw new ArgumentException($"GraphType <{setting.Type}> is not found.");
                }

                graphs[setting.Id] = graph;
            }

            foreach (var setting in json.Connections)
            {
                var from = GetNode(graphs, setting.From);
                var to = GetNode(graphs, setting.To);

                var result = setting.Connect ? connector.ConnectNode(from, to) : connector.DisconnectNode(from, to);
                if (!result)
                {
                    throw new EvaluateException($"Failed to {(setting.Connect ? "dis":"")}connect node : {setting.From} & {setting.To}.");
                }
            }

            foreach (var setting in json.Executions)
            {
                if (!graphs.ContainsKey(setting.GraphId))
                {
                    throw new ArgumentException($"Graph <{setting.GraphId}> is not found.");
                }
                
                IGraph graph = graphs[setting.GraphId];
                if (graph is UpdaterGraph updater)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        await updater.Update(0);
                    }
                }
                else{
                    throw new ArgumentException($"Graph <{setting.GraphId}> is not UpdaterGraph.");
                }
            }

        }

        private static INode GetNode(IDictionary<int, IGraph> graphs, NodeInfo info)
        {
            if (!graphs.ContainsKey(info.GraphId))
            {
                throw new ArgumentException($"Graph <{info.GraphId}> is not found.");
            }

            var graph = graphs[info.GraphId];
            INode node;
            
            switch (info.Type)
            {
                case 0:
                    if (graph.InProcessNodes.Count <= info.Index)
                    {
                        throw new ArgumentException($"Node <{info}> is not found.");
                    }

                    node = graph.InProcessNodes[info.Index];
                    break;
                case 1:
                    if (graph.OutProcessNodes.Count <= info.Index)
                    {
                        throw new ArgumentException($"Node <{info}> is not found.");
                    }

                    node = graph.OutProcessNodes[info.Index];
                    break;
                case 2:
                    if (graph.InItemNodes.Count <= info.Index)
                    {
                        throw new ArgumentException($"Node <{info}> is not found.");
                    }

                    node = graph.InItemNodes[info.Index];
                    break;
                case 3:
                    if (graph.OutItemNodes.Count <= info.Index)
                    {
                        throw new ArgumentException($"Node <{info}> is not found.");
                    }

                    node = graph.OutItemNodes[info.Index];
                    break;
                default:
                    throw new ArgumentException($"Unexpected Node Type {info.GraphId}.{info.Type}");
            }

            return node;
        }

        public static Task<bool> PrintText(string message)
        {
            Console.WriteLine(message);
            return Task.FromResult(true);
        }

        static void CreateTemplate()
        {
            var a = new ExecutionSetting();
            a.Graphs = new List<GraphSetting>();
            a.Connections = new List<NodeConnection>();
            a.Executions = new List<Execution>();

            a.Graphs.Add(new GraphSetting());
            a.Graphs[0].Id = 1;
            a.Graphs[0].Type = "Updater";
            a.Graphs[0].Setting = new Dictionary<string, string>();

            a.Graphs[0].Setting["Time"] = "1";

            Console.WriteLine(JsonSerializer.Serialize(a));
        }
    }
}