using System;
using System.Threading.Tasks;

namespace PipelineNet.Middleware
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility middleware.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the middleware.</typeparam>
    /// <typeparam name="TReturn">The return type of the middleware.</typeparam>
    public interface IAsyncMiddleware<TParameter, TReturn>
    {
        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="next">The next middleware in the flow.</param>
        /// <returns>The return value.</returns>
        Task<TReturn> Run(TParameter parameter, Func<TParameter, Task<TReturn>> next);
    }
}
