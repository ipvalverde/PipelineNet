using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNet.ChainsOfResponsibility
{
    public interface IAsyncResponsibilityChain<TParameter, TReturn>
    {
        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}<"/>.
        /// </summary>
        /// <param name="finallyFunc">The <see cref="Func{TParameter, Task{TResult}}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc);

        /// <summary>
        /// Chain a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter, TReturn>;

        /// <summary>
        /// Execute the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        Task<TReturn> Execute(TParameter parameter);
    }
}
