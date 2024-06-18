using Microsoft.Extensions.DependencyInjection;
using PipelineNet.ChainsOfResponsibility;
using PipelineNet.Extensions.DependencyInjection.Pipelines;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.Extensions.DependencyInjection.Internal.ChainsOfResponsibility
{
    internal class AsyncResponsibilityChainBuilder<TParameter, TReturn> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter>>, IAsyncResponsibilityChainBuilder<TParameter, TReturn>
    {
        private readonly IServiceCollection _services;
        private readonly Action<IAsyncResponsibilityChain<TParameter, TReturn>> _configure;

        public AsyncResponsibilityChainBuilder(
            IServiceCollection services,
            Action<IAsyncResponsibilityChain<TParameter, TReturn>> configure = null)
        {
            _services = services;
            _configure = configure;
        }

        public IAsyncResponsibilityChainBuilder<TParameter, TReturn> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient
        {
            _services.AddTransient<TClient, TImplementation>(sp =>
            {
                var middlewareResolver = sp.GetRequiredService<IMiddlewareResolver>();
                var pipeline = new AsyncResponsibilityChain<TParameter, TReturn>(middlewareResolver);

                _configure?.Invoke(pipeline);

                var typedClientFactory = sp.GetRequiredService<ITypedClientFactory<TImplementation, IAsyncResponsibilityChain<TParameter, TReturn>>>();
                return typedClientFactory.CreateClient(pipeline);
            });

            return this;
        }
    }
}
