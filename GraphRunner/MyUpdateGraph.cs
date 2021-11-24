using System;
using System.Threading;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Nodes;

namespace GraphRunner
{
    public class MyUpdateGraph : Graph
    {
        private SerialSender _sender;

        public MyUpdateGraph(INodeConnector connector, SerialSender sender) : base(connector)
        {
            _sender = sender;
        }

        public override Task<ProcessCallResult> OnProcessCall(ProcessCallArgs args, object[] parameters)
        {
            return Task.FromResult(ProcessCallResult.Success(null,OutProcessNodes[0]));
        }

        public async Task Execute(Func<string, Task<bool>> func)
        {
            var args = ProcessCallArgs.Fire(this);
            args.SetDummyData("PrintMessageFunc",func);
            
            await _sender.Fire(args,this);
        }

        public override string GraphName => "MyUpdateGraph";
    }
    public class SerialSender
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public async Task Fire(ProcessCallArgs args, IGraph graph)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await graph.InvokeWithoutCheck(args, true, null);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}