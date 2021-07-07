using System;
using System.Threading.Tasks;

namespace PipelineNet.Middleware
{
    public interface IAsyncMiddleware<TParameter, TReturn>
    {
        Task<TReturn> Run(TParameter parameter, Func<TParameter, Task<TReturn>> next,object args = null);
    }
}
