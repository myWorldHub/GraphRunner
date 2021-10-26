using System.Threading;
using System.Threading.Tasks;
using GraphConnectEngine;

namespace GraphRunner
{
    public class SerialSender
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        
        public async Task Fire(ProcessCallArgs args,IGraph graph)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await graph.InvokeWithoutCheck(args,true,null);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}