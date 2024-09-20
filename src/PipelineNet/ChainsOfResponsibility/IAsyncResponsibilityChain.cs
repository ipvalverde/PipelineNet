using PipelineNet.Finally;
using PipelineNet.Middleware;
using System;
using System.Threading;
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
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally.
        /// </summary>
        /// <typeparam name="TFinally">The finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Finally<TFinally>()
            where TFinally : IAsyncFinally<TParameter, TReturn>;

        /// <summary>
        /// Sets the cancellable finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally.
        /// </summary>
        /// <typeparam name="TCancellableFinally">The cancellable finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> CancellableFinally<TCancellableFinally>()
            where TCancellableFinally : ICancellableAsyncFinally<TParameter, TReturn>;

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally.
        /// </summary>
        /// <param name="finallyType">The <see cref="IAsyncFinally{TParameter, TReturn}"/> or <see cref="ICancellableAsyncFinally{TParameter, TReturn}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Finally(Type finallyType);

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="finallyFunc">The function that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        [Obsolete("This overload is obsolete. Use Finally<TFinally> or CancellableFinally<TCancellableFinally>.")]
        IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc);

        /// <summary>
        /// Chains a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter, TReturn>;

        /// <summary>
        /// Chains a new middleware type to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IAsyncMiddleware{TParameter, TReturn}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        IAsyncResponsibilityChain<TParameter, TReturn> Chain(Type middlewareType);

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        Task<TReturn> Execute(TParameter parameter);

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken">The cancellation token that will be passed to all middleware.</param>
        Task<TReturn> Execute(TParameter parameter, CancellationToken cancellationToken);
    }
}
