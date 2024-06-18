using Microsoft.Extensions.DependencyInjection;
using PipelineNet.Extensions.DependencyInjection.Pipelines;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.Extensions.DependencyInjection.Internal.Pipelines
{
    internal class PipelineBuilder<TParameter> : BaseMiddlewareFlow<IMiddleware<TParameter>>, IPipelineBuilder<TParameter>
    {
        private readonly IServiceCollection _services;
        private readonly Action<IPipeline<TParameter>> _configure;

        public PipelineBuilder(
            IServiceCollection services,
            Action<IPipeline<TParameter>> configure = null)
        {
            _services = services;
            _configure = configure;
        }

        public IPipelineBuilder<TParameter> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient
        {
            _services.AddTransient<TClient, TImplementation>(sp =>
            {
                var middlewareResolver = sp.GetRequiredService<IMiddlewareResolver>();
                var pipeline = new Pipeline<TParameter>(middlewareResolver);

                _configure?.Invoke(pipeline);

                var typedClientFactory = sp.GetRequiredService<ITypedClientFactory<TImplementation, IPipeline<TParameter>>>();
                return typedClientFactory.CreateClient(pipeline);
            });

            return this;
        }
    }
}
