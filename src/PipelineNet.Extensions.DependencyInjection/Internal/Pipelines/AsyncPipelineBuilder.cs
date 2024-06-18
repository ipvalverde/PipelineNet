using Microsoft.Extensions.DependencyInjection;
using PipelineNet.Extensions.DependencyInjection.Pipelines;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;

namespace PipelineNet.Extensions.DependencyInjection.Internal.Pipelines
{
    internal class AsyncPipelineBuilder<TParameter> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter>>, IAsyncPipelineBuilder<TParameter>
    {
        private readonly IServiceCollection _services;
        private readonly Action<IAsyncPipeline<TParameter>> _configure;

        public AsyncPipelineBuilder(
            IServiceCollection services,
            Action<IAsyncPipeline<TParameter>> configure = null)
        {
            _services = services;
            _configure = configure;
        }

        public IAsyncPipelineBuilder<TParameter> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient
        {
            _services.AddTransient<TClient, TImplementation>(sp =>
            {
                var middlewareResolver = sp.GetRequiredService<IMiddlewareResolver>();
                var pipeline = new AsyncPipeline<TParameter>(middlewareResolver);

                _configure?.Invoke(pipeline);

                var typedClientFactory = sp.GetRequiredService<ITypedClientFactory<TImplementation, IAsyncPipeline<TParameter>>>();
                return typedClientFactory.CreateClient(pipeline);
            });

            return this;
        }
    }
}
