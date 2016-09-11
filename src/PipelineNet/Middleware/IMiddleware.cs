using System;

namespace PipelineNet.Middleware
{
    public interface IMiddleware<TParameter>
    {
        void Run(TParameter parameter, Action<TParameter> next);
    }
}
