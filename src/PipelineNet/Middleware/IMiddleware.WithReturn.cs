using System;

namespace PipelineNet.Middleware
{
    /// <summary>
    /// Defines the chain of responsibility middleware.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the middleware.</typeparam>
    /// <typeparam name="TReturn">The return type of the middleware.</typeparam>
    public interface IMiddleware<TParameter, TReturn>
    {
        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="next">The next middleware in the flow.</param>
        /// <returns>The return value.</returns>
        TReturn Run(TParameter parameter, Func<TParameter, TReturn> next);
    }
}
