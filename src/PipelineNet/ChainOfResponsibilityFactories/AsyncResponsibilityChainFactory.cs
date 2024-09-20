using PipelineNet.ChainsOfResponsibility;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ChainOfResponsibilityFactories
{
    /// <inheritdoc/>
    public class AsyncResponsibilityChainFactory<TParameter, TReturn>
        : IAsyncResponsibilityChainFactory<TParameter, TReturn>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        /// <summary>
        /// Creates a new <see cref="AsyncResponsibilityChainFactory{TParameter, TReturn}"/>.
        /// </summary>
        /// <param name="middlewareResolver">The <see cref="IMiddlewareResolver"/>.</param>
        public AsyncResponsibilityChainFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        /// <inheritdoc/>
        public IAsyncResponsibilityChain<TParameter, TReturn> Create() =>
            new AsyncResponsibilityChain<TParameter, TReturn>(_middlewareResolver);
    }
}
