using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.PipelineFactories
{
    public class AsyncPipelineFactory<TParameter>
        : IAsyncPipelineFactory<TParameter>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        public AsyncPipelineFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        public IAsyncPipeline<TParameter> Create() =>
            new AsyncPipeline<TParameter>(_middlewareResolver);
    }
}
