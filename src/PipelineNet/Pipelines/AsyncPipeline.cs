using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public class AsyncPipeline<TParameter> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter>>, IAsyncPipeline<TParameter>
        where TParameter : class
    {
        public AsyncPipeline(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {}

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public IAsyncPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter>
        {
            MiddlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        public IAsyncPipeline<TParameter> Add(Type middlewareType)
        {
            base.AddMiddleware(middlewareType);
            return this;
        }

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        public async Task Execute(TParameter parameter)
        {
            if (MiddlewareTypes.Count == 0)
                return;

            int index = 0;
            Func<TParameter, Task> action = null;
            action = async (param) =>
            {
                var type = MiddlewareTypes[index];
                var resolverResult = MiddlewareResolver.Resolve(type);
                var middleware = (IAsyncMiddleware<TParameter>)resolverResult.Middleware;

                index++;
                if (index == MiddlewareTypes.Count)
                    action = (p) => Task.FromResult(0);

                await middleware.Run(param, action).ConfigureAwait(false);

                if (resolverResult.IsDisposable)
                {
#if NETSTANDARD2_1_OR_GREATER
                    if (middleware is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    else
                    {
#endif
                        ((IDisposable)middleware).Dispose();
#if NETSTANDARD2_1_OR_GREATER
                    }
#endif
                }
            };

            await action(parameter).ConfigureAwait(false);
        }
    }
}
