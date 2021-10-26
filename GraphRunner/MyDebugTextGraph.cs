using System;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Nodes;

namespace GraphRunner
{
    public class MyDebugTextGraph : Graph
    {
        
        public MyDebugTextGraph(INodeConnector connector) : base(connector)
        {
            AddNode(new InItemNode(this,new ItemTypeResolver(typeof(object),"Message")));
        }

        public override async Task<ProcessCallResult> OnProcessCall(ProcessCallArgs args, object[] parameters)
        {
            var func = args.GetDummyData("PrintMessageFunc") as Func<string,Task<bool>>;
            if (func == null)
            {
                return ProcessCallResult.Fail();
            }
            else
            {
                var result = await func(parameters[0].ToString());
                return ProcessCallResult.Success(null,OutProcessNodes[0]);
            }
        }

        public override string GraphName => "MyDebugText";
    }
}