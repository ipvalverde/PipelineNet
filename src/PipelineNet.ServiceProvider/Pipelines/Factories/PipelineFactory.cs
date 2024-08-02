using PipelineNet.Pipelines;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.Pipelines.Factories
{
    /// <inheritdoc/>
    public class PipelineFactory<TParameter>
        : IPipelineFactory<TParameter>
    {
        /// <inheritdoc/>
        public IPipeline<TParameter> Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return new Pipeline<TParameter>(
                new ServiceProviderMiddlewareResolver(serviceProvider));
        }
    }
}
