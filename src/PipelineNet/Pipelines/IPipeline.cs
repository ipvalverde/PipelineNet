using PipelineNet.Middleware;

namespace PipelineNet.Pipelines
{
    public interface IPipeline<TParameter>
        where TParameter : class
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter>;
    }
}
