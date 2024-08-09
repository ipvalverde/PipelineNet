using System;
using System.Threading.Tasks;

namespace PipelineNet.Middleware
{
    /// <summary>
    /// Defines the asynchronous pipeline middleware.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for the middleware.</typeparam>
    public interface IAsyncMiddleware<TParameter>
    {
        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="next">The next middleware in the flow.</param>
        Task Run(TParameter parameter, Func<TParameter, Task> next);
    }
}
