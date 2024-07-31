using PipelineNet.ChainsOfResponsibility;

namespace PipelineNet.PipelineFactories
{
    /// <summary>
    /// Used to create new instances of a chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public interface IResponsibilityChainFactory<TParameter, TReturn>
    {
        /// <summary>
        /// Creates a new chain of responsibility.
        /// </summary>
        /// <returns>A new instance of a chain.</returns>
        IResponsibilityChain<TParameter, TReturn> Create();
    }
}
