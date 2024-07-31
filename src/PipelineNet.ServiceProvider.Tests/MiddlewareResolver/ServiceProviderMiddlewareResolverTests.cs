using Microsoft.Extensions.DependencyInjection;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using PipelineNet.Middleware;

namespace PipelineNet.ServiceProvider.Tests.MiddlewareResolver
{
    public class ServiceProviderMiddlewareResolverTests
    {
        #region Service defintions
        public interface ITransientService
        {
        }

        public class TransientService : ITransientService
        {
        }

        public interface IScopedService
        {
        }

        public class ScopedService : IScopedService
        {
        }

        public interface ISingletonService
        {
        }

        public class SingletonService : ISingletonService
        {
        }
        #endregion

        #region Middleware definitions
        public class ParameterlessConstructorMiddleware : IMiddleware<object>
        {
            public void Run(object parameter, Action<object> next)
            {
            }
        }

        public class TransientMiddleware : IMiddleware<object>
        {
            public ITransientService Service { get; }

            public TransientMiddleware(ITransientService service)
            {
                Service = service;
            }

            public void Run(object parameter, Action<object> next)
            {
            }
        }

        public class ScopedMiddleware : IMiddleware<object>
        {
            public IScopedService Service { get; }

            public ScopedMiddleware(IScopedService service)
            {
                Service = service;
            }

            public void Run(object parameter, Action<object> next)
            {
            }
        }

        public class SingletonMiddleware : IMiddleware<object>
        {
            public ISingletonService Service { get; }

            public SingletonMiddleware(ISingletonService service)
            {
                Service = service;
            }

            public void Run(object parameter, Action<object> next)
            {
            }
        }

        public class DisposableMiddleware : IMiddleware<object>, IDisposable
        {
            public void Run(object parameter, Action<object> next)
            {
            }

            public void Dispose()
            {
            }
        }
        #endregion

        [Fact]
        public void Resolve_ResolvesParameterlessConstructorMiddleware()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<ParameterlessConstructorMiddleware>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ServiceProviderMiddlewareResolver(serviceProvider);

            var resolverResult = resolver.Resolve(typeof(ParameterlessConstructorMiddleware));

            Assert.NotNull(resolverResult.Middleware);
            Assert.False(resolverResult.IsDisposable);
        }

        [Fact]
        public void Resolve_ResolvesTransientMiddleware()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<TransientMiddleware>()
                .AddTransient<ITransientService, TransientService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ServiceProviderMiddlewareResolver(serviceProvider);

            var resolverResult = resolver.Resolve(typeof(TransientMiddleware));

            Assert.NotNull(resolverResult.Middleware);
            Assert.False(resolverResult.IsDisposable);
        }

        [Fact]
        public void Resolve_ResolvesScopedMiddleware()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<ScopedMiddleware>()
                .AddScoped<IScopedService, ScopedService>()
                .BuildServiceProvider(validateScopes: true);
            var scope = serviceProvider.CreateScope();
            var resolver = new ServiceProviderMiddlewareResolver(scope.ServiceProvider);

            var resolverResult = resolver.Resolve(typeof(ScopedMiddleware));

            Assert.NotNull(resolverResult.Middleware);
            Assert.False(resolverResult.IsDisposable);
        }

        [Fact]
        public void Resolve_ResolvesSingletonMiddleware()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<SingletonMiddleware>()
                .AddSingleton<ISingletonService, SingletonService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ServiceProviderMiddlewareResolver(serviceProvider);

            var resolverResult = resolver.Resolve(typeof(SingletonMiddleware));

            Assert.NotNull(resolverResult.Middleware);
            Assert.False(resolverResult.IsDisposable);
        }

        [Fact]
        public void Resolve_ResolvesDisposableMiddleware()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<DisposableMiddleware>()
                .AddSingleton<ISingletonService, SingletonService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ServiceProviderMiddlewareResolver(serviceProvider);

            var resolverResult = resolver.Resolve(typeof(DisposableMiddleware));

            Assert.NotNull(resolverResult.Middleware);
            Assert.False(resolverResult.IsDisposable);
        }

        [Fact]
        public void Resolve_ThrowsInvalidOperationExceptionWhenServiceIsNotRegistered()
        {
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ServiceProviderMiddlewareResolver(serviceProvider);

            Assert.Throws<InvalidOperationException>(() =>
                resolver.Resolve(typeof(TransientMiddleware)));
        }
    }
}
