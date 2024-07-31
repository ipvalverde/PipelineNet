using PipelineNet.Pipelines;

namespace PipelineNet.PipelineFactories
{
    /// <summary>
    /// Used to create new instances of a pipeline.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public interface IPipelineFactory<TParameter>
    {
        /// <summary>
        /// Creates a new pipeline.
        /// </summary>
        /// <returns>A new instance of a pipeline.</returns>
        IPipeline<TParameter> Create();
    }
}
