using PipelineNet.Pipelines;
using System;

namespace PipelineNet.ServiceProvider.Pipelines.Factories
{
    /// <summary>
    /// Used to create new instances of pipeline.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public interface IPipelineFactory<TParameter>
    {
        /// <summary>
        /// Creates a new pipeline.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> that all the middleware will be resolved from.</param>
        /// <returns>A new instance of pipeline.</returns>
        IPipeline<TParameter> Create(IServiceProvider serviceProvider);
    }
}
