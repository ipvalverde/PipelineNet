using Microsoft.Extensions.DependencyInjection;
﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using PipelineNet.Middleware;
using PipelineNet.ServiceProvider.ChainsOfResponsibility.Factories;
using PipelineNet.ServiceProvider.Pipelines.Factories;
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
        /// Adds pipeline and chain of responsibility factories. Adds all middleware from assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPipelineNet(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            services.AddMiddlewareFlowFactories();
            services.AddMiddlewareFromAssemblies(assemblies, lifetime);

            return services;
        }

        /// <summary>
        /// Adds pipeline and chain of responsibility factories. Adds all middleware from the assambly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddPipelineNet(
            this IServiceCollection services,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assembly == null) throw new ArgumentNullException("assembly");

            return services.AddPipelineNet(new[] { assembly }, lifetime);
        }

        /// <summary>
        /// Adds pipeline and chain of responsibility factories.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMiddlewareFlowFactories(
            this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException("services");

            services.TryAddSingleton(typeof(IPipelineFactory<>), typeof(PipelineFactory<>));
            services.TryAddSingleton(typeof(IAsyncPipelineFactory<>), typeof(AsyncPipelineFactory<>));
            services.TryAddSingleton(typeof(IResponsibilityChainFactory<,>), typeof(ResponsibilityChainFactory<,>));
            services.TryAddSingleton(typeof(IAsyncResponsibilityChainFactory<,>), typeof(AsyncResponsibilityChainFactory<,>));

            return services;
        }

        /// <summary>
        /// Adds all middleware from assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <param name="lifetime">The lifetime of the registered middleware.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMiddlewareFromAssemblies(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            foreach (var assembly in assemblies)
            {
                services.AddMiddlewareFromAssembly(assembly, lifetime);
            }

            return services;
        }

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
            ServiceLifetime lifetime = ServiceLifetime.Transient)
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
                    services.TryAdd(new ServiceDescriptor(type, type, lifetime));
                }
            }

            return services;
        }
    }
}
