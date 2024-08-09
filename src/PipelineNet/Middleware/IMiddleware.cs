using System;

namespace PipelineNet.Middleware
{
    /// <summary>
    /// Defines the pipeline middleware.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for the middleware.</typeparam>
    public interface IMiddleware<TParameter>
    {
        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="next">The next middleware in the flow.</param>
        void Run(TParameter parameter, Action<TParameter> next);
    }
}
