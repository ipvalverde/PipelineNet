using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.ServiceProvider.Pipelines.Factories
{
    /// <inheritdoc/>
    public class AsyncPipelineFactory<TParameter>
        : IAsyncPipelineFactory<TParameter>
    {
        private readonly IMiddlewareResolver _middlewareResolver;

        /// <summary>
        /// Creates a new <see cref="AsyncPipelineFactory{TParameter}"/>.
        /// </summary>
        /// <param name="middlewareResolver">The <see cref="IMiddlewareResolver"/>.</param>
        public AsyncPipelineFactory(IMiddlewareResolver middlewareResolver)
        {
            _middlewareResolver = middlewareResolver ?? throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided.");
        }

        /// <inheritdoc/>
        public IAsyncPipeline<TParameter> Create() =>
            new AsyncPipeline<TParameter>(_middlewareResolver);
    }
}
