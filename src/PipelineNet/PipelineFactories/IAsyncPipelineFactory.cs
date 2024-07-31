using PipelineNet.Pipelines;

namespace PipelineNet.PipelineFactories
{
    /// <summary>
    /// Used to create new instances of an asynchronous pipeline.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public interface IAsyncPipelineFactory<TParameter>
    {
        /// <summary>
        /// Creates a new asynchronous pipeline.
        /// </summary>
        /// <returns>A new instance of an asynchronous pipeline.</returns>
        IAsyncPipeline<TParameter> Create();
    }
}
