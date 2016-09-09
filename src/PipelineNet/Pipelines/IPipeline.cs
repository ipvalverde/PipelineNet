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

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        void Execute(TParameter parameter);
    }
}
