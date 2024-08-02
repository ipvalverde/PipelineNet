using PipelineNet.ChainsOfResponsibility;
using System;

namespace PipelineNet.ServiceProvider.ChainsOfResponsibility.Factories
{
    /// <summary>
    /// Used to create new instances of chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public interface IResponsibilityChainFactory<TParameter, TReturn>
    {
        /// <summary>
        /// Creates a new chain of responsibility.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> that all the middleware will be resolved from.</param>
        /// <returns>A new instance of chain.</returns>
        IResponsibilityChain<TParameter, TReturn> Create(IServiceProvider serviceProvider);
    }
}
