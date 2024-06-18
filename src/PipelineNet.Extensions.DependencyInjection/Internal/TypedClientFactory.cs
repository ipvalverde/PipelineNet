using Microsoft.Extensions.DependencyInjection;
using System;

namespace PipelineNet.Extensions.DependencyInjection.Internal
{
    internal class TypedClientFactory<TClient, TMiddlewareFlow> : ITypedClientFactory<TClient, TMiddlewareFlow>
    {
        private readonly Cache _cache;
        private readonly IServiceProvider _serviceProvider;

        public TypedClientFactory(Cache cache, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
        }

        public TClient CreateClient(TMiddlewareFlow middlewareFlow)
        {
            return (TClient)_cache.Activator(_serviceProvider, new object[] { middlewareFlow });
        }

        public class Cache
        {
            public ObjectFactory Activator { get; }

            public Cache()
            {
                Activator = ActivatorUtilities.CreateFactory(typeof(TClient), new[] { typeof(TMiddlewareFlow) });
            }
        }
    }
}
