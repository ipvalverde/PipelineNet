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

        public class TestOutputHelperLogger : ILogger
        {
            private readonly ITestOutputHelper _output;

            public TestOutputHelperLogger(ITestOutputHelper output)
            {
                _output = output;
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull =>
                new NullScope();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                _output.WriteLine($"{logLevel}:{eventId}:{formatter(state, exception)}");

            private class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }

        public class TestOutputHelperLoggerProvider : ILoggerProvider
        {
            private readonly ITestOutputHelper _output;

            public TestOutputHelperLoggerProvider(ITestOutputHelper output)
            {
                _output = output;
            }

            public ILogger CreateLogger(string categoryName) =>
                new TestOutputHelperLogger(_output);

            public void Dispose()
            {
            }
        }
        #endregion

        #region Middleware definitions
        public class RoudCornersAsyncMiddleware : IAsyncMiddleware<Bitmap>
        {
            private readonly ILogger<RoudCornersAsyncMiddleware> _logger;

            // The following constructor arguments will be provided by the IServiceProvider
            public RoudCornersAsyncMiddleware(ILogger<RoudCornersAsyncMiddleware> logger)
            {
                _logger = logger;
            }

            public async Task Run(Bitmap parameter, Func<Bitmap, Task> next)
            {
                _logger.LogInformation("Running RoudCornersAsyncMiddleware.");
                // Handle somehow
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
                .AddLogging(builder => builder.Services.AddSingleton<ILoggerProvider>(new TestOutputHelperLoggerProvider(_output)))
                .BuildServiceProvider(validateScopes: true);
            var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMyService>();

            await service.DoSomething();
        }
    }
}
