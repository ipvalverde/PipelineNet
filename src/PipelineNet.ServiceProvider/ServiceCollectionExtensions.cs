using Microsoft.Extensions.DependencyInjection;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using PipelineNet.PipelineFactories;
using PipelineNet.ServiceProvider.MiddlewareResolver;
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
        /// Adds core PipelineNet services and all middleware from the assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies scan.</param>
        /// <param name="lifetime">The lifetime of the registered services and middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPipelineNet(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            services.AddPipelineNetCore(lifetime);
            services.AddMidlewaresFromAssemblies(assemblies, lifetime);

            return services;
        }

        /// <summary>
        /// Adds core PipelineNet services and all middleware from the assambly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly scan.</param>
        /// <param name="lifetime">The lifetime of the registered services and middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPipelineNet(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assembly == null) throw new ArgumentNullException("assembly");

            return services.AddPipelineNet(new[] { assembly }, lifetime);
        }

        /// <summary>
        /// Adds core PipelineNet services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="lifetime">The lifetime of the registered services.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPipelineNetCore(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null) throw new ArgumentNullException("services");

            services.Add(new ServiceDescriptor(typeof(IMiddlewareResolver), typeof(ServiceProviderMiddlewareResolver), lifetime));
            services.Add(new ServiceDescriptor(typeof(IPipelineFactory<>), typeof(PipelineFactory<>), lifetime));
            services.Add(new ServiceDescriptor(typeof(IAsyncPipelineFactory<>), typeof(AsyncPipelineFactory<>), lifetime));
            services.Add(new ServiceDescriptor(typeof(IResponsibilityChainFactory<,>), typeof(ResponsibilityChainFactory<,>), lifetime));
            services.Add(new ServiceDescriptor(typeof(IAsyncResponsibilityChainFactory<,>), typeof(AsyncResponsibilityChainFactory<,>), lifetime));

            return services;
        }

        /// <summary>
        /// Adds all middleware from the assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMidlewaresFromAssemblies(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            foreach (var assembly in assemblies)
            {
                services.AddMidlewaresFromAssembly(assembly, lifetime);
            }

            return services;
        }

        /// <summary>
        /// Adds all middleware from the assembly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMidlewaresFromAssembly(
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
                typeof(IAsyncMiddleware<,>)
            };

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.IsAbstract
                    && !type.IsGenericTypeDefinition
                    && type.GetInterfaces().Any(i => i.IsGenericType && openGenericTypes.Contains(i.GetGenericTypeDefinition())))
                {
                    services.Add(new ServiceDescriptor(type, type, lifetime));
                }
            }

            return services;
        }
    }
}
