using PipelineNet.ChainsOfResponsibility;

namespace PipelineNet.PipelineFactories
{
    /// <summary>
    /// Used to create new instances of an asynchronous chain of responsibility.
    /// </summary
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public interface IAsyncResponsibilityChainFactory<TParameter, TReturn>
    {
        /// <summary>
        /// Creates a new asynchronous chain of responsibility.
        /// </summary>
        /// <returns>A new instance of an asynchronous chain.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Create();
    }
}
