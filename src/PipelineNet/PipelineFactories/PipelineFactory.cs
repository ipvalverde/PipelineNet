using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.PipelineFactories
{
    public class PipelineFactory<TParameter>
        : IPipelineFactory<TParameter>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        public PipelineFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        public IPipeline<TParameter> Create() =>
            new Pipeline<TParameter>(_middlewareResolver);
    }
}
