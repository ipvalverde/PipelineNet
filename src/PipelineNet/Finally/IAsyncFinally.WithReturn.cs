using System.Threading.Tasks;

namespace PipelineNet.Finally
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility finally.
    /// Finally will be executed at the end of the chain as a fallback.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the finally.</typeparam>
    /// <typeparam name="TReturn">The return type of the finally.</typeparam>
    public interface IAsyncFinally<TParameter, TReturn>
    {
        /// <summary>
        /// Executes the finally.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>The return value.</returns>
        Task<TReturn> Finally(TParameter parameter);
    }
}
