using Microsoft.Extensions.DependencyInjection;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.Extensions.DependencyInjection.MiddlewareResolver
{
    /// <summary>
    /// An implementation of <see cref="IMiddlewareResolver"/> that retrieves
    /// instances from <see cref="IServiceProvider"/>.
    /// </summary>
    public class ServiceProviderMiddlewareResolver : IMiddlewareResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderMiddlewareResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException("serviceProvider",
                "An instance of IServiceProvider must be provided."); ;
        }

        public MiddlewareResolverResult Resolve(Type type)
        {
            var middleware = _serviceProvider.GetRequiredService(type);

            return new MiddlewareResolverResult()
            {
                Middleware = middleware,
                IsDisposable = false
            };
        }
    }
}
