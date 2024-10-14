using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PipelineNet.Finally;
using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PipelineNet.ServiceProvider
{
    /// <summary>
    /// Extension methods to the service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all middleware from the assembly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMiddlewareFromAssembly(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var openGenericTypes = new List<Type>()
            {
                typeof(IMiddleware<>),
                typeof(IAsyncMiddleware<>),
                typeof(IMiddleware<,>),
                typeof(IAsyncMiddleware<,>),
                typeof(IFinally<,>),
                typeof(ICancellableAsyncFinally<,>),
                typeof(IAsyncFinally<,>)
            };

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.IsAbstract
                    && !type.IsGenericTypeDefinition
                    && type.GetInterfaces().Any(i => i.IsGenericType && openGenericTypes.Contains(i.GetGenericTypeDefinition())))
                {
                    services.TryAdd(new ServiceDescriptor(type, type, lifetime));
                }
            }

            return services;
        }
    }
}
