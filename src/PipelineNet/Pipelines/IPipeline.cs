using PipelineNet.Middleware;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// A pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public interface IPipeline<TParameter>
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IPipeline<TParameter> Add<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IMiddleware<TParameter>, new();
        IPipeline<TParameter> Add<TMiddleware>([NotNull]TMiddleware middleware)
            where TMiddleware : IMiddleware<TParameter>, new();
        /// <summary>
        /// Executes the configured pipeline.
        /// </summary>
        /// <param name="parameter">The input that will be provided to all middleware.</param>
        void Execute(TParameter parameter);

    }
}
