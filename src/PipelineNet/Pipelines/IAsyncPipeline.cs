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
        IAsyncPipeline<TParameter> Add<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IAsyncMiddleware<TParameter>, new();

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        Task Execute(TParameter parameter);

    }
}
