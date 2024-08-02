using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PipelineNet.Middleware;
using PipelineNet.Pipelines;
using PipelineNet.ServiceProvider.Pipelines.Factories;
using Xunit.Abstractions;

namespace PipelineNet.ServiceProvider.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        #region Service defintions
        public class Bitmap
        {
        }

        public interface IMyService
        {
            Task DoSomething();
        }

        public class MyService : IMyService
        {
            private readonly IAsyncPipelineFactory<Bitmap> _pipelineFactory;
            private readonly IServiceProvider _serviceProvider;

            public MyService(
                IAsyncPipelineFactory<Bitmap> pipelineFactory,
                IServiceProvider serviceProvider)
            {
                _pipelineFactory = pipelineFactory;
                _serviceProvider = serviceProvider;
            }

            public async Task DoSomething()
            {
                Bitmap image = new Bitmap();

                IAsyncPipeline<Bitmap> pipeline = _pipelineFactory.Create(_serviceProvider)
                    .Add<RoudCornersAsyncMiddleware>()
                    .Add<AddTransparencyAsyncMiddleware>()
                    .Add<AddWatermarkAsyncMiddleware>();

                await pipeline.Execute(image);
            }
        }

        public class TestOutputHelperLogger<TCategoryName> : ILogger<TCategoryName>
        {
            private readonly ITestOutputHelper _output;

            public TestOutputHelperLogger(ITestOutputHelper output)
            {
                _output = output;
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _output.WriteLine($"{logLevel}:{eventId}:{formatter(state, exception)}");
            }
        }
        #endregion

        #region Middleware definitions
        public class RoudCornersAsyncMiddleware : IAsyncMiddleware<Bitmap>
        {
            private readonly ILogger<RoudCornersAsyncMiddleware> _logger;

            public RoudCornersAsyncMiddleware(ILogger<RoudCornersAsyncMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Run(Bitmap parameter, Func<Bitmap, Task> next)
            {
                _logger.LogInformation("Running RoudCornersAsyncMiddleware.");
                await next(parameter);
            }
        }

        public class AddTransparencyAsyncMiddleware : IAsyncMiddleware<Bitmap>
        {
            private readonly ILogger<AddTransparencyAsyncMiddleware> _logger;

            public AddTransparencyAsyncMiddleware(ILogger<AddTransparencyAsyncMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Run(Bitmap parameter, Func<Bitmap, Task> next)
            {
                _logger.LogInformation("Running AddTransparencyAsyncMiddleware.");
                await next(parameter);
            }
        }

        public class AddWatermarkAsyncMiddleware : IAsyncMiddleware<Bitmap>
        {
            private readonly ILogger<AddWatermarkAsyncMiddleware> _logger;

            public AddWatermarkAsyncMiddleware(ILogger<AddWatermarkAsyncMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Run(Bitmap parameter, Func<Bitmap, Task> next)
            {
                _logger.LogInformation("Running AddWatermarkAsyncMiddleware.");
                await next(parameter);
            }
        }
        #endregion


        private readonly ITestOutputHelper _output;

        public ServiceCollectionExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task AcceptanceTest()
        {
            var serviceProvider = new ServiceCollection()
                .AddPipelineNet(typeof(RoudCornersAsyncMiddleware).Assembly)
                .AddScoped<IMyService, MyService>()
                .AddSingleton(_output)
                .AddSingleton(typeof(ILogger<>), typeof(TestOutputHelperLogger<>))
                .BuildServiceProvider(validateScopes: true);
            var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMyService>();

            await service.DoSomething();
        }
    }
}
