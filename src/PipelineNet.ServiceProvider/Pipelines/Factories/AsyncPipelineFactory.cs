using PipelineNet.Pipelines;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.Pipelines.Factories
{
    /// <inheritdoc/>
    public class AsyncPipelineFactory<TParameter>
        : IAsyncPipelineFactory<TParameter>
    {
        /// <inheritdoc/>
        public IAsyncPipeline<TParameter> Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return new AsyncPipeline<TParameter>(
                new ServiceProviderMiddlewareResolver(serviceProvider));
        }
    }
}
