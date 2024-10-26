using PipelineNet.Middleware;
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
    public interface IAsyncPipeline<TParameter>
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IAsyncPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter>;

        /// <summary>
        /// Adds a cancellable middleware type to be executed.
        /// </summary>
        /// <typeparam name="TCancellableMiddleware"></typeparam>
        /// <returns></returns>
        IAsyncPipeline<TParameter> AddCancellable<TCancellableMiddleware>()
            where TCancellableMiddleware : ICancellableAsyncMiddleware<TParameter>;

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        Task Execute(TParameter parameter);

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken">The cancellation token that will be passed to all middleware.</param>
        Task Execute(TParameter parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/> or <see cref="ICancellableAsyncMiddleware{TParameter}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        IAsyncPipeline<TParameter> Add(Type middlewareType);
    }
}
