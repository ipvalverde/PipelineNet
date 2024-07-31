using PipelineNet.ChainsOfResponsibility;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.PipelineFactories
{
    public class AsyncResponsibilityChainFactory<TParameter, TReturn>
        : IAsyncResponsibilityChainFactory<TParameter, TReturn>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        public AsyncResponsibilityChainFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        public IAsyncResponsibilityChain<TParameter, TReturn> Create() =>
            new AsyncResponsibilityChain<TParameter, TReturn>(_middlewareResolver);
    }
}
