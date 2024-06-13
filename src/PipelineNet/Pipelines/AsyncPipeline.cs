using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// An asynchronous pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public class AsyncPipeline<TParameter> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter>>, IAsyncPipeline<TParameter>
    {
        public AsyncPipeline(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        { }

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
                MiddlewareResolverResult resolverResult = null;
                try
                {
                    var type = MiddlewareTypes[index];
                    resolverResult = MiddlewareResolver.Resolve(type);
                    var middleware = (IAsyncMiddleware<TParameter>)resolverResult.Middleware;

                    index++;
                    if (index == MiddlewareTypes.Count)
                        action = (p) => Task.FromResult(0);

                    if (resolverResult.IsDisposable && !(middleware is IDisposable
#if NETSTANDARD2_1_OR_GREATER
                        || middleware is IAsyncDisposable
#endif
                        ))
                    {
                        throw new InvalidOperationException($"'{middleware.GetType().FullName}' type does not implement IDisposable" +
#if NETSTANDARD2_1_OR_GREATER
                            " or IAsyncDisposable" +
#endif
                            ".");
                    }

                    await middleware.Run(param, action).ConfigureAwait(false);
                }
                finally
                {
                    if (resolverResult != null && resolverResult.IsDisposable)
                    {
                        var middleware = resolverResult.Middleware;
                        if (middleware != null)
                        {
#if NETSTANDARD2_1_OR_GREATER
                            if (middleware is IAsyncDisposable asyncDisposable)
                            {
                                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                            }
                            else
#endif
                            if (middleware is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                }
            };

            await action(parameter).ConfigureAwait(false);
        }
    }
}
