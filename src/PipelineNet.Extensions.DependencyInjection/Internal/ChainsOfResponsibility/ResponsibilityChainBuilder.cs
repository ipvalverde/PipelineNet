using Microsoft.Extensions.DependencyInjection;
using PipelineNet.ChainsOfResponsibility;
using PipelineNet.Extensions.DependencyInjection.Pipelines;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.Extensions.DependencyInjection.Internal.ChainsOfResponsibility
{
    internal class ResponsibilityChainBuilder<TParameter, TReturn> : BaseMiddlewareFlow<IMiddleware<TParameter>>, IResponsibilityChainBuilder<TParameter, TReturn>
    {
        private readonly IServiceCollection _services;
        private readonly Action<IResponsibilityChain<TParameter, TReturn>> _configure;

        public ResponsibilityChainBuilder(
            IServiceCollection services,
            Action<IResponsibilityChain<TParameter, TReturn>> configure = null)
        {
            _services = services;
            _configure = configure;
        }

        public IResponsibilityChainBuilder<TParameter, TReturn> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient
        {
            _services.AddTransient<TClient, TImplementation>(sp =>
            {
                var middlewareResolver = sp.GetRequiredService<IMiddlewareResolver>();
                var chain = new ResponsibilityChain<TParameter, TReturn>(middlewareResolver);

                _configure?.Invoke(chain);

                var typedClientFactory = sp.GetRequiredService<ITypedClientFactory<TImplementation, IResponsibilityChain<TParameter, TReturn>>>();
                return typedClientFactory.CreateClient(chain);
            });

            return this;
        }
    }
}
