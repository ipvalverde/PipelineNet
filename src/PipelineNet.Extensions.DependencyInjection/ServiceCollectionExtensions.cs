using Microsoft.Extensions.DependencyInjection;
using PipelineNet.ChainsOfResponsibility;
using PipelineNet.Extensions.DependencyInjection.Internal;
using PipelineNet.Extensions.DependencyInjection.Internal.ChainsOfResponsibility;
using PipelineNet.Extensions.DependencyInjection.Internal.Pipelines;
using PipelineNet.Extensions.DependencyInjection.MiddlewareResolver;
using PipelineNet.Extensions.DependencyInjection.Pipelines;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PipelineNet.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds middlewares from the specified assemblies and core PipelineNet services.
        /// </summary>
        public static IServiceCollection AddPipelineNet(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton(typeof(TypedClientFactory<,>.Cache));
            services.AddTransient(typeof(ITypedClientFactory<,>), typeof(TypedClientFactory<,>));
            services.AddTransient<IMiddlewareResolver, ServiceProviderMiddlewareResolver>();

            foreach (var assembly in assemblies)
            {
                services.AddMidlewaresFromAssembly(assembly);
            }

            return services;
        }

        private static void AddMidlewaresFromAssembly(this IServiceCollection services, Assembly assembly)
        {
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
                    services.AddTransient(type);
                }
            }
        }

        /// <summary>
        /// Gets a pipeline builder that forwards calls to the underlying service collection.
        /// </summary>
        public static IPipelineBuilder<TParameter> AddPipeline<TParameter>(
            this IServiceCollection services,
            Action<IPipeline<TParameter>> configure = null)
        {
            return new PipelineBuilder<TParameter>(services, configure);
        }

        /// <summary>
        /// Gets an asynchronous pipeline builder that forwards calls to the underlying service collection.
        /// </summary>
        public static IAsyncPipelineBuilder<TParameter> AddAsyncPipeline<TParameter>(
            this IServiceCollection services,
            Action<IAsyncPipeline<TParameter>> configure = null)
        {
            return new AsyncPipelineBuilder<TParameter>(services, configure);
        }

        /// <summary>
        /// Gets a chain of responsibility builder that forwards calls to the underlying service collection.
        /// </summary>
        public static IResponsibilityChainBuilder<TParameter, TReturn> AddResponsibilityChain<TParameter, TReturn>(
            this IServiceCollection services,
            Action<IResponsibilityChain<TParameter, TReturn>> configure = null)
        {
            return new ResponsibilityChainBuilder<TParameter, TReturn>(services, configure);
        }

        /// <summary>
        /// Gets an asynchronous chain of responsibility builder that forwards calls to the underlying service collection.
        /// </summary>
        public static IAsyncResponsibilityChainBuilder<TParameter, TReturn> AddAsyncResponsibilityChain<TParameter, TReturn>(
            this IServiceCollection services,
            Action<IAsyncResponsibilityChain<TParameter, TReturn>> configure = null)
        {
            return new AsyncResponsibilityChainBuilder<TParameter, TReturn>(services, configure);
        }
    }
}
