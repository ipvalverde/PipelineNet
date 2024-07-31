using PipelineNet.ChainsOfResponsibility;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.PipelineFactories
{
    public class ResponsibilityChainFactory<TParameter, TReturn>
        : IResponsibilityChainFactory<TParameter, TReturn>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        public ResponsibilityChainFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        public IResponsibilityChain<TParameter, TReturn> Create() =>
            new ResponsibilityChain<TParameter, TReturn>(_middlewareResolver);
    }
}
