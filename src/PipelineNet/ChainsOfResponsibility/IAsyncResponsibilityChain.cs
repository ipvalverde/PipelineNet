using PipelineNet.Middleware;
using System;
using System.Threading.Tasks;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public interface IAsyncResponsibilityChain<TParameter, TReturn>
    {
        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="finallyFunc">The function that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc);

        /// <summary>
        /// Chains a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IAsyncMiddleware<TParameter, TReturn>, new();

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        Task<TReturn> Execute(TParameter parameter);
    }
}
