using System;

namespace PipelineNet.Middleware
{
    public interface IMiddleware<TParameter, TResult>
    {
        TResult Run(TParameter parameter, Func<TParameter, TResult> next);
    }

    public interface IMiddleware<TParameter>
    {
        void Run(TParameter parameter, Action<TParameter> next);
    }
}
