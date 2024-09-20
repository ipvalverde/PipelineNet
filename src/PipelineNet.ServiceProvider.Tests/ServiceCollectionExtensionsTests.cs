using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PipelineNet.Middleware;
using PipelineNet.PipelineFactories;
using PipelineNet.Pipelines;
using Xunit.Abstractions;

namespace PipelineNet.ServiceProvider.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        #region Parameter definitions
        public class Image
        {
            public static Image FromFile(string filename) => new Bitmap();
        }

        public class Bitmap : Image
        {
        }
        #endregion

        #region Service definitions
        public interface IMyService
        {
            Task DoSomething();
        }

        public class MyService : IMyService
        {
            private readonly IAsyncPipelineFactory<Bitmap> _pipelineFactory;

            public MyService(IAsyncPipelineFactory<Bitmap> pipelineFactory)
            {
                _pipelineFactory = pipelineFactory;
            }

            public async Task DoSomething()
            {
                IAsyncPipeline<Bitmap> pipeline = _pipelineFactory.Create()
                    .Add<RoudCornersAsyncMiddleware>()
                    .Add<AddTransparencyAsyncMiddleware>()
                    .Add<AddWatermarkAsyncMiddleware>();

                Bitmap image = (Bitmap) Image.FromFile("party-photo.png");

                await pipeline.Execute(image);
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
                // Handle somehow
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
                // Handle somehow
                await next(parameter);
            }
        }
        #endregion

        #region Logger definitions
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

        public class TestOutputHelperLogger : ILogger
        {
            private readonly ITestOutputHelper _output;

            public TestOutputHelperLogger(ITestOutputHelper output)
            {
                _output = output;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        new NullScope();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                _output.WriteLine($"{eventId}:{logLevel}:{formatter(state, exception)}");

            private class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
        #endregion

        private readonly ITestOutputHelper _output;

        public ServiceCollectionExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task AddPipelineNet_Works_Readme()
        {
            var serviceProvider = new ServiceCollection()
                .AddPipelineNet(typeof(RoudCornersAsyncMiddleware).Assembly)
                .AddScoped<IMyService, MyService>()
                .AddLogging(bulider => bulider.Services.AddSingleton<ILoggerProvider>(new TestOutputHelperLoggerProvider(_output)))
                .BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMyService>();

            await service.DoSomething();
        }
    }
}
