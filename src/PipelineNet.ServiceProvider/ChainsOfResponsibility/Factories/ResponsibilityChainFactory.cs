using PipelineNet.ChainsOfResponsibility;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.ChainsOfResponsibility.Factories
{
    /// <inheritdoc/>
    public class ResponsibilityChainFactory<TParameter, TReturn>
        : IResponsibilityChainFactory<TParameter, TReturn>
    {
        /// <inheritdoc/>
        public IResponsibilityChain<TParameter, TReturn> Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return new ResponsibilityChain<TParameter, TReturn>(
                new ServiceProviderMiddlewareResolver(serviceProvider));
        }
    }
}
