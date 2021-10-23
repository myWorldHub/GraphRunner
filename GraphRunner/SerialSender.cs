using System.Threading;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Graphs.Event;

namespace GraphRunner
{
    public class SerialSender : IProcessSender
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        
        public async Task Fire(IGraph graph, object[] parameters)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await graph.InvokeWithoutArgs(true, parameters);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}