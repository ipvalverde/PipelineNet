using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Threading.Tasks;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public class AsyncResponsibilityChain<TParameter, TReturn> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter, TReturn>>,
        IAsyncResponsibilityChain<TParameter, TReturn>
    {
        private Func<TParameter, Task<TReturn>> _finallyFunc;

        /// <summary>
        /// Creates a new asynchronous chain of responsibility.
        /// </summary>
        /// <param name="middlewareResolver">The resolver used to create the middleware types.</param>
        public AsyncResponsibilityChain(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {
        }

        /// <summary>
        /// Chains a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>() where TMiddleware : IAsyncMiddleware<TParameter, TReturn>
        {
            MiddlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Chains a new middleware type to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IAsyncMiddleware{TParameter, TReturn}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Chain(Type middlewareType)
        {
            base.AddMiddleware(middlewareType);
            return this;
        }

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public async Task<TReturn> Execute(TParameter parameter)
        {
            if (MiddlewareTypes.Count == 0)
                return default(TReturn);

            int index = 0;
            Func<TParameter, Task<TReturn>> func = null;
            func = async (param) =>
            {
                var type = MiddlewareTypes[index];
                var resolverResult = MiddlewareResolver.Resolve(type);
                var middleware = (IAsyncMiddleware<TParameter, TReturn>)resolverResult.Middleware;

                index++;
                // If the current instance of middleware is the last one in the list,
                // the "next" function is assigned to the finally function or a 
                // default empty function.
                if (index == MiddlewareTypes.Count)
                    func = this._finallyFunc ?? ((p) => Task.FromResult(default(TReturn)));

                var result = await middleware.Run(param, func);

                if (resolverResult.IsDisposable)
                {
#if NETSTANDARD2_1_OR_GREATER
                    if (middleware is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    else
                    {
#endif
                        ((IDisposable)middleware).Dispose();
#if NETSTANDARD2_1_OR_GREATER
                    }
#endif
                }

                return result;
            };

            return await func(parameter).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}<"/>.
        /// </summary>
        /// <param name="finallyFunc">The function that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc)
        {
            this._finallyFunc = finallyFunc;
            return this;
        }
    }
}
