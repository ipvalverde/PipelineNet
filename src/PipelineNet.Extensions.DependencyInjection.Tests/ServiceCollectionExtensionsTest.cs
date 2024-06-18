using Microsoft.Extensions.DependencyInjection;
using PipelineNet.Middleware;
using PipelineNet.Pipelines;
using System.Reflection;

namespace PipelineNet.Extensions.DependencyInjection.Tests
{
    public class ServiceCollectionExtensionsTest
    {
        #region Helpers
        public static class MiddlewareTypesHelper
        {
            public static IList<Type> GetMiddlewareTypes<TMiddleware>(BaseMiddlewareFlow<TMiddleware> middlewareFlow)
            {
                var property = typeof(BaseMiddlewareFlow<TMiddleware>).GetProperty("MiddlewareTypes", BindingFlags.Instance | BindingFlags.NonPublic);
                return (IList<Type>)property!.GetValue(middlewareFlow)!;
            }
        }
        #endregion

        #region Service defintions
        public interface IMyPipeline
        {
            IList<Type> GetMiddlewareTypes();
        }

        public class MyPipeline1 : IMyPipeline
        {
            private readonly IPipeline<object> _pipeline1;

            public MyPipeline1(IPipeline<object> pipeline1)
            {
                _pipeline1 = pipeline1;
            }

            public IList<Type> GetMiddlewareTypes()
            {
                return MiddlewareTypesHelper.GetMiddlewareTypes((BaseMiddlewareFlow<IMiddleware<object>>)_pipeline1);
            }
        }

        public interface IMyPipeline2
        {
            IList<Type> GetMiddlewareTypes();
        }

        public class MyPipeline2 : IMyPipeline2
        {
            private readonly IPipeline<object> _pipeline1;

            public MyPipeline2(IPipeline<object> pipeline1)
            {
                _pipeline1 = pipeline1;
            }

            public IList<Type> GetMiddlewareTypes() =>
                MiddlewareTypesHelper.GetMiddlewareTypes((BaseMiddlewareFlow<IMiddleware<object>>)_pipeline1);
        }

        public interface IMyPipeline3
        {
            IList<Type> GetMiddlewareTypes();
        }

        public class MyPipeline3 : IMyPipeline3
        {
            private readonly IPipeline<object> _pipeline2;

            public MyPipeline3(IPipeline<object> pipeline2)
            {
                _pipeline2 = pipeline2;
            }

            public IList<Type> GetMiddlewareTypes() =>
                MiddlewareTypesHelper.GetMiddlewareTypes((BaseMiddlewareFlow<IMiddleware<object>>)_pipeline2);
        }
        #endregion

        #region Middleware definitions
        public class Middleware1 : IMiddleware<object>
        {
            public void Run(object parameter, Action<object> next)
            {
            }
        }

        public class Middleware2 : IMiddleware<object>
        {
            public void Run(object parameter, Action<object> next)
            {
            }
        }
        #endregion

        [Fact]
        public void AddTypedClient_Works()
        {
            var services = new ServiceCollection();
            services.AddPipelineNet(typeof(Middleware1).Assembly);

            services
                .AddPipeline<object>(pipeline1 =>
                {
                    pipeline1
                        .Add<Middleware1>()
                        .Add<Middleware2>();
                })
                .AddTypedClient<IMyPipeline, MyPipeline1>()
                .AddTypedClient<IMyPipeline2, MyPipeline2>();

            services
                .AddPipeline<object>(pipeline2 =>
                {
                    pipeline2
                        .Add<Middleware1>();
                })
                .AddTypedClient<IMyPipeline3, MyPipeline3>();

            var serviceProvider = services.BuildServiceProvider(validateScopes: true);

            var pipeline1 = serviceProvider.GetRequiredService<IMyPipeline>();
            var middlewareTypes1 = pipeline1.GetMiddlewareTypes();

            var pipeline2 = serviceProvider.GetRequiredService<IMyPipeline2>();
            var middlewareTypes2 = pipeline2.GetMiddlewareTypes();

            var pipeline3 = serviceProvider.GetRequiredService<IMyPipeline3>();
            var middlewareTypes3 = pipeline3.GetMiddlewareTypes();

            Assert.Equal(2, middlewareTypes1.Count);
            Assert.Equal(typeof(Middleware1), middlewareTypes1[0]);
            Assert.Equal(typeof(Middleware2), middlewareTypes1[1]);

            Assert.Equal(2, middlewareTypes2.Count);
            Assert.Equal(typeof(Middleware1), middlewareTypes2[0]);
            Assert.Equal(typeof(Middleware2), middlewareTypes2[1]);

            Assert.Single(middlewareTypes3);
            Assert.Equal(typeof(Middleware1), middlewareTypes3[0]);
        }
    }
}
