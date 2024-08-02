using Microsoft.Extensions.DependencyInjection;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.MiddlewareResolver
{
    /// <summary>
    /// An implementation of <see cref="IMiddlewareResolver"/> that resolves
    /// instances using the <see cref="IServiceProvider"/>.
    /// </summary>
    public class ServiceProviderMiddlewareResolver : IMiddlewareResolver
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new <see cref="ServiceProviderMiddlewareResolver"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ServiceProviderMiddlewareResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException("serviceProvider",
                "An instance of IServiceProvider must be provided."); ;
        }

        /// <summary>
        /// Resolves an instance of the give middleware type.
        /// </summary>
        /// <param name="type">The middleware type that will be resolved.</param>
        /// <returns>An instance of the middleware.</returns>
        public MiddlewareResolverResult Resolve(Type type)
        {
            var middleware = _serviceProvider.GetRequiredService(type);
            bool isDisposable = false;

            return new MiddlewareResolverResult()
            {
                Middleware = middleware,
                IsDisposable = isDisposable
            };
        }
    }
}
