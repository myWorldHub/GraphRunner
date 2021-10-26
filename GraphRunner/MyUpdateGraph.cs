using System;
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

        public async Task Execute()
        {
            Func<string,Task<bool>> func = async (msg) =>
            {
                Console.WriteLine(msg);
                return true;
            };

            var args = ProcessCallArgs.Fire(this);
            args.SetDummyData("PrintMessageFunc",func);
            
            await _sender.Fire(args,this);
        }

        public override string GraphName => "MyUpdateGraph";
    }
}