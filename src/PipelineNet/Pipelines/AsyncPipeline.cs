using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// An asynchronous pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public class AsyncPipeline<TParameter> : AsyncBaseMiddlewareFlow<IAsyncMiddleware<TParameter>, ICancellableAsyncMiddleware<TParameter>>, IAsyncPipeline<TParameter>
    {
        /// <summary>
        /// Creates a new instance of asynchronous Pipeline.
        /// </summary>
        /// <param name="middlewareResolver">Resolver responsible for resolving instances out of middleware types.</param>
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
        /// Adds a cancellable middleware type to be executed.
        /// </summary>
        /// <typeparam name="TCancellableMiddleware"></typeparam>
        /// <returns></returns>
        public IAsyncPipeline<TParameter> AddCancellable<TCancellableMiddleware>()
            where TCancellableMiddleware : ICancellableAsyncMiddleware<TParameter>
        {
            MiddlewareTypes.Add(typeof(TCancellableMiddleware));
            return this;
        }

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/> or <see cref="ICancellableAsyncMiddleware{TParameter}"/>.</exception>
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
        public async Task Execute(TParameter parameter) =>
            await Execute(parameter, default).ConfigureAwait(false);

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken">The cancellation token that will be passed to all middleware.</param>
        public async Task Execute(TParameter parameter, CancellationToken cancellationToken)
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

                    index++;
                    if (index == MiddlewareTypes.Count)
                        action = async (p) => await Task.FromResult(default(int)).ConfigureAwait(false);

                    EnsureMiddlewareNotNull(resolverResult, type);
                    await RunMiddlewareAsync(resolverResult, param, action, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await DisposeMiddlewareAsync(resolverResult).ConfigureAwait(false);
                }
            };

            await action(parameter).ConfigureAwait(false);
        }

        private static async Task RunMiddlewareAsync(
            MiddlewareResolverResult resolverResult,
            TParameter parameter,
            Func<TParameter, Task> next,
            CancellationToken cancellationToken)
        {
            if (resolverResult.Middleware is ICancellableAsyncMiddleware<TParameter> cancellableMiddleware)
            {
                await cancellableMiddleware.Run(parameter, next, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var middleware = (IAsyncMiddleware<TParameter>)resolverResult.Middleware;
                await middleware.Run(parameter, next).ConfigureAwait(false);
            }
        }
    }
}
