using PipelineNet.ChainsOfResponsibility;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ChainOfResponsibilityFactories
{
    /// <inheritdoc/>
    public class ResponsibilityChainFactory<TParameter, TReturn>
        : IResponsibilityChainFactory<TParameter, TReturn>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        /// <summary>
        /// Creates a new <see cref="ResponsibilityChainFactory{TParameter, TReturn}"/>.
        /// </summary>
        /// <param name="middlewareResolver">The <see cref="IMiddlewareResolver"/>.</param>
        public ResponsibilityChainFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        /// <inheritdoc/>
        public IResponsibilityChain<TParameter, TReturn> Create() =>
            new ResponsibilityChain<TParameter, TReturn>(_middlewareResolver);
    }
}
