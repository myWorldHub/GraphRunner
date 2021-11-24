#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;

namespace GraphRunner
{
    public class ExecutionEnv : IDisposable
    {
        private readonly INodeConnector _connector;

        private readonly IDictionary<int, IGraph> _graphs;
        private readonly IDictionary<string, int> _path2execution;

        private HttpListener _httpListener;
        
        public bool IsServerRunning { get; private set; }
        
        public int Port { get; private set; }

        public ExecutionEnv()
        {
            _graphs = new Dictionary<int, IGraph>();
            _connector = new NodeConnector();
            _path2execution = new Dictionary<string, int>();
            
            IsServerRunning = false;
        }

        public bool AddGraph(int id,GraphSetting setting)
        {
            if (_graphs.ContainsKey(id)) return false;

            var graph = CreateGraph(setting);

            if (graph != null)
            {
                _graphs[id] = graph;
                return true;
            }

            return false;
        }

        public bool RemoveGraph(int id)
        {
            if (!_graphs.ContainsKey(id))
                return false;
            
            _graphs[id].Dispose();
            _graphs.Remove(id);
            return true;
        }

        public bool ConnectNode(NodeData nodeData1,NodeData nodeData2,bool connect = true)
        {
            var node1 = GetNode(nodeData1);
            var node2 = GetNode(nodeData2);

            if (node1 == null || node2 == null)
                return false;

            return connect ? _connector.ConnectNode(node1, node2) : _connector.DisconnectNode(node1,node2);
        }

        public async Task<bool> Execute(int graphId, Func<string, Task<bool>> func)
        {
            if (!_graphs.ContainsKey(graphId))
            {
                return false;
            }

            var graph = _graphs[graphId];
            if (graph is MyUpdateGraph updater)
            {
                await updater.Execute(func);
                return true;
            }

            return false;
        }

        public bool AddPath(string path,int graphId)
        {
            if (_path2execution.ContainsKey(path))
            {
                return false;
            }
            
            _path2execution[path] = graphId;
            return true;
        }

        public bool RemovePath(string path)
        {
            if (_path2execution.ContainsKey(path))
            {
                _path2execution.Remove(path);
                return true;
            }
            return false;
        }

        public bool StartServer(int port)
        {
            if (IsServerRunning)
            {
                return false;
            }

            IsServerRunning = true;
            Port = port;
            
            //TODO ホストを指定できるように
            SimpleListener(new MyLogger(),new []{$@"http://localhost:{port}/"});
            
            return true;
        }

        public bool StopServer()
        {
            if (IsServerRunning)
            {
                try
                {
                    _httpListener?.Stop();
                }
                catch (ObjectDisposedException)
                {
                    
                }
                IsServerRunning = false;
                return true;
            }

            return false;
        }

        // This example requires the System and System.Net namespaces.
        private async Task SimpleListener(ILogger logger,string[] prefixes)
        {
            logger.WriteLine("Starting server...");
            
            if (!HttpListener.IsSupported)
            {
                logger.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            _httpListener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                _httpListener.Prefixes.Add(s);
            }
            try
            {
                _httpListener.Start();
                logger.WriteLine("Started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
            
            while (IsServerRunning)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = await _httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                
                logger.WriteLine($"Access from {context.Request.RemoteEndPoint} to {request.RawUrl}");
                
                var sw = new Stopwatch();
                sw.Start();

                if (request.Url == null)
                {
                    sw.Stop();
                    
                    var res = context.Response;
                    res.StatusCode = 400;
                    res.Close();
                    
                    logger.WriteLine($"Completed 404 NotFound in {sw.ElapsedMilliseconds}ms");
                    continue;
                }

                var path = request.Url.LocalPath;

                if (!_path2execution.ContainsKey(path))
                {
                    sw.Stop();

                    var res = context.Response;
                    res.StatusCode = 404;
                    res.Close();

                    logger.WriteLine($"Completed 404 NotFound in {sw.ElapsedMilliseconds}ms");
                    continue;
                }

                var graphId = _path2execution[path];

                if (!_graphs.ContainsKey(graphId))
                {
                    sw.Stop();
                    
                    var res = context.Response;
                    res.StatusCode = 500;
                    res.Close();
                    
                    logger.WriteLine($"Completed 500 Internal Server Error in {sw.ElapsedMilliseconds}ms\n" +
                                     $"Could not find Graph[id:{graphId}]");
                    continue;
                }

                if (!(_graphs[graphId] is MyUpdateGraph updater))
                {
                    sw.Stop();
                    
                    var res = context.Response;
                    res.StatusCode = 500;
                    res.Close();

                    logger.WriteLine($"Completed 500 Internal Server Error in {sw.ElapsedMilliseconds}ms" + 
                                     $"Graph[id:{graphId}] is not Updater");
                    continue;
                }
                
                // Obtain a response object.
                var response = context.Response;
                System.IO.Stream output = response.OutputStream;
                
                ListenExec(logger,sw,output, response, updater);
            }
        }

        private async void ListenExec(ILogger logger,Stopwatch sw,System.IO.Stream stream, HttpListenerResponse response,MyUpdateGraph updater)
        {
            // Construct a response.
            string responseString = "";

            await updater.Execute(async (msg) =>
            {
                responseString += msg;
                return true;
            });
            
            sw.Stop();
            logger.WriteLine($"Completed 200 OK in {sw.ElapsedMilliseconds}ms");
            
            
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            await stream.WriteAsync(buffer, 0, buffer.Length);

            // You must close the output stream.
            stream.Close();
        }

        public INode? GetNode(NodeData info)
        {
            return GetNode(info.GraphId, info.NodeType, info.Index);
        }

        public INode? GetNode(int graphId, NodeType type, int index)
        {
            if (!_graphs.ContainsKey(graphId))
            {
                return null;
            }

            var graph = _graphs[graphId];

            switch (type)
            {
                case NodeType.InProcess:
                    return graph.InProcessNodes.Count <= index ? null : graph.InProcessNodes[index];
                case NodeType.OutProcess:
                    return graph.OutProcessNodes.Count <= index ? null : graph.OutProcessNodes[index];
                case NodeType.InItem:
                    return graph.InItemNodes.Count <= index ? null : graph.InItemNodes[index];
                case NodeType.OutItem:
                    return graph.OutItemNodes.Count <= index ? null : graph.OutItemNodes[index];
            }

            return null;
        }


        private IGraph? CreateGraph(GraphSetting setting)
        {
            switch (setting.Type)
            {
                case "Updater":
                    return new MyUpdateGraph(_connector, new SerialSender());
                case "PrintText":
                    return new MyDebugTextGraph(_connector);
                case "Value":

                    if (!(setting.Setting.ContainsKey("Type") &&
                          setting.Setting.ContainsKey("Value")))
                        return null;

                    switch (setting.Setting["Type"])
                    {
                        case "Int":
                            return new ValueGraph<int>(_connector, int.Parse(setting.Setting["Value"]));
                    }

                    break;
            }

            return null;
        }

        public void Dispose()
        {
            _httpListener?.Stop();
            IsServerRunning = false;
        }
    }
}