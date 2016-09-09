using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public interface IAsyncPipeline<TParameter>
        where TParameter : class
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IAsyncPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter>;

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        Task Execute(TParameter parameter);
    }
}
