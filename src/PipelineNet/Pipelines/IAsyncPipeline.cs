using PipelineNet.Middleware;
using System;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public interface IAsyncPipeline<TParameter>
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IAsyncPipeline<TParameter> Add<TMiddleware>(object args = null)
            where TMiddleware : IAsyncMiddleware<TParameter>;

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        Task Execute(TParameter parameter);

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        IAsyncPipeline<TParameter> Add(Type middlewareType,object args = null);
    }
}
