using Microsoft.Extensions.DependencyInjection;
using PipelineNet.ServiceProvider.MiddlewareResolver;
using PipelineNet.Middleware;

namespace PipelineNet.ServiceProvider.Tests.MiddlewareResolver
{
    public class ActivatorUtilitiesMiddlewareResolverTest
    {
        #region Service defintions
        public interface ITransientService
        {
            bool Disposed { get; }
        }

        public class TransientService : ITransientService, IDisposable
        {
            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        public interface IScopedService
        {
            bool Disposed { get; }
        }

        public class ScopedService : IScopedService, IDisposable
        {
            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        public interface ISingletonService
        {
            bool Disposed { get; }
        }

        public class SingletonService : ISingletonService, IDisposable
        {
            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }
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
        #endregion

        [Fact]
        public void Resolve_ResolvesParameterlessConstructor()
        {
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ActivatorUtilitiesMiddlewareResolver(serviceProvider);

            var middleware = resolver.Resolve(typeof(ParameterlessConstructorMiddleware));

            Assert.NotNull(middleware);
        }

        [Fact]
        public void Resolve_ResolvesTransient()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<ITransientService, TransientService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ActivatorUtilitiesMiddlewareResolver(serviceProvider);

            var middleware = resolver.Resolve(typeof(TransientMiddleware));

            Assert.NotNull(middleware);
        }

        [Fact]
        public void Resolve_ResolvesScoped()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IScopedService, ScopedService>()
                .BuildServiceProvider(validateScopes: true);
            var scope = serviceProvider.CreateScope();
            var resolver = new ActivatorUtilitiesMiddlewareResolver(scope.ServiceProvider);

            var middleware = resolver.Resolve(typeof(ScopedMiddleware));

            Assert.NotNull(middleware);
        }

        [Fact]
        public void Resolve_ResolvesSingleton()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISingletonService, SingletonService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ActivatorUtilitiesMiddlewareResolver(serviceProvider);

            var middleware = resolver.Resolve(typeof(SingletonMiddleware));

            Assert.NotNull(middleware);
        }

        [Fact]
        public void Resolve_TransientGetsDisposed()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<ITransientService, TransientService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ActivatorUtilitiesMiddlewareResolver(serviceProvider);
            
            var middleware = (TransientMiddleware)resolver.Resolve(typeof(TransientMiddleware));
            serviceProvider.Dispose();

            Assert.True(middleware.Service.Disposed);
        }

        [Fact]
        public void Resolve_ScopedGetsDisposed()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IScopedService, ScopedService>()
                .BuildServiceProvider(validateScopes: true);
            var scope = serviceProvider.CreateScope();
            var resolver = new ActivatorUtilitiesMiddlewareResolver(scope.ServiceProvider);

            var middleware = (ScopedMiddleware)resolver.Resolve(typeof(ScopedMiddleware));
            scope.Dispose();

            Assert.True(middleware.Service.Disposed);
        }

        [Fact]
        public void Resolve_SingletonGetsDisposed()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISingletonService, SingletonService>()
                .BuildServiceProvider(validateScopes: true);
            var resolver = new ActivatorUtilitiesMiddlewareResolver(serviceProvider);

            var middleware = (SingletonMiddleware)resolver.Resolve(typeof(SingletonMiddleware));
            serviceProvider.Dispose();

            Assert.True(middleware.Service.Disposed);
        }
    }
}
