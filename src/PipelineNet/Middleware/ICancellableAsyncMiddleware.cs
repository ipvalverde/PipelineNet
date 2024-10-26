using System;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineNet.Middleware
{
    /// <summary>
    /// Defines the asynchronous pipeline middleware with cancellation token.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for the middleware.</typeparam>
    public interface ICancellableAsyncMiddleware<TParameter>
    {
        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="next">The next middleware in the flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The return value.</returns>
        Task Run(TParameter parameter, Func<TParameter, Task> next, CancellationToken cancellationToken);
    }
}
