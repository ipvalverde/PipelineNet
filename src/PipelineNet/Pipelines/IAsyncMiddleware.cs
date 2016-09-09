using System;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public interface IAsyncMiddleware<TParameter, TResult>
    {
        Task<TResult> Run(TParameter parameter, Func<TParameter, Task<TResult>> next);
    }

    public interface IAsyncMiddleware<TParameter>
    {
        Task Run(TParameter parameter, Func<TParameter, Task> next);
    }
}
