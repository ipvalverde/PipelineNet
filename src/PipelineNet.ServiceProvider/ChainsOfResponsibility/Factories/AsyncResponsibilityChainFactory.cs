using PipelineNet.ChainsOfResponsibility;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.ChainsOfResponsibility.Factories
{
    /// <inheritdoc/>
    public class AsyncResponsibilityChainFactory<TParameter, TReturn>
        : IAsyncResponsibilityChainFactory<TParameter, TReturn>
    {
        /// <inheritdoc/>
        public IAsyncResponsibilityChain<TParameter, TReturn> Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return new AsyncResponsibilityChain<TParameter, TReturn>(
                new ServiceProviderMiddlewareResolver(serviceProvider));
        }
    }
}
