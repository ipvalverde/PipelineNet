using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.ServiceProvider.Pipelines.Factories
{
    /// <inheritdoc/>
    public class PipelineFactory<TParameter>
        : IPipelineFactory<TParameter>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        /// <summary>
        /// Creates a new <see cref="PipelineFactory{TParameter}"/>.
        /// </summary>
        /// <param name="middlewareResolver">The <see cref="IMiddlewareResolver"/>.</param>
        public PipelineFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        /// <inheritdoc/>
        public IPipeline<TParameter> Create() =>
            new Pipeline<TParameter>(_middlewareResolver);
    }
}
