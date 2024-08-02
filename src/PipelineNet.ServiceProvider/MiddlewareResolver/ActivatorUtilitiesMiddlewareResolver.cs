using Microsoft.Extensions.DependencyInjection;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.MiddlewareResolver
{
    /// <summary>
    /// An implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the <see cref="ActivatorUtilities"/>.
    /// </summary>
    public class ActivatorUtilitiesMiddlewareResolver : IMiddlewareResolver
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new <see cref="ActivatorUtilitiesMiddlewareResolver"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ActivatorUtilitiesMiddlewareResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException("serviceProvider",
                "An instance of IServiceProvider must be provided."); ;
        }

        /// <inheritdoc/>
        public MiddlewareResolverResult Resolve(Type type)
        {
            var middleware = ActivatorUtilities.CreateInstance(_serviceProvider, type);
            bool isDisposable = middleware is IDisposable
#if NETSTANDARD2_1_OR_GREATER
                || middleware is IAsyncDisposable
#endif
                ;

            return new MiddlewareResolverResult()
            {
                Middleware = middleware,
                IsDisposable = isDisposable
            };
        }
    }
}
