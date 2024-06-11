using Microsoft.Extensions.DependencyInjection;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ServiceProvider.MiddlewareResolver
{
    /// <summary>
    /// An implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the <see cref="Microsoft.Extensions.DependencyInjection.ActivatorUtilities"/>.
    /// </summary>
    public class ActivatorUtilitiesMiddlewareResolver : IMiddlewareResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ActivatorUtilitiesMiddlewareResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException("serviceProvider",
                "An instance of IServiceProvider must be provided."); ;
        }

        public object Resolve(Type type)
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
