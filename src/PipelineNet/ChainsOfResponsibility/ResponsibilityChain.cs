using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public class ResponsibilityChain<TParameter, TReturn> : BaseMiddlewareFlow<IMiddleware<TParameter, TReturn>>,
        IResponsibilityChain<TParameter, TReturn>
    {
        private Func<TParameter, TReturn> _finallyFunc;

        /// <summary>
        /// Creates a new chain of responsibility.
        /// </summary>
        /// <param name="middlewareResolver"></param>
        public ResponsibilityChain(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {
        }

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="finallyFunc">The <see cref="Func{TParameter, TResult}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, TReturn> finallyFunc)
        {
            this._finallyFunc = finallyFunc;
            return this;
        }

        /// <summary>
        /// Chains a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter, TReturn>
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
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Chain(Type middlewareType)
        {
            base.AddMiddleware(middlewareType);
            return this;
        }

        /// <summary>
        /// Execute the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public TReturn Execute(TParameter parameter)
        {
            if (MiddlewareTypes.Count == 0)
                return default(TReturn);

            int index = 0;
            Func<TParameter, TReturn> func = null;
            func = (param) =>
            {
                var type = MiddlewareTypes[index];
                var resolverResult = MiddlewareResolver.Resolve(type);
                var middleware = (IMiddleware<TParameter, TReturn>)resolverResult.Middleware;

                index++;
                // If the current instance of middleware is the last one in the list,
                // the "next" function is assigned to the finally function or a 
                // default empty function.
                if (index == MiddlewareTypes.Count)
                    func = this._finallyFunc ?? ((p) => default(TReturn));

                var result = middleware.Run(param, func);

                if (resolverResult.IsDisposable)
                {
                    if (middleware is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
#if NETSTANDARD2_1_OR_GREATER
                    else if (middleware is IAsyncDisposable)
                    {
                        throw new InvalidOperationException($"'{middleware.GetType().FullName}' type only implements IAsyncDisposable. " +
                            "Use AsyncResponsibilityChain to execute the configured chain of responsibility.");
                    }
#endif
                    else
                    {
                        throw new InvalidOperationException($"'{middleware.GetType().FullName}' type does not implement IDisposable " +
#if NETSTANDARD2_1_OR_GREATER
                            " or IAsyncDisposable" +
#endif
                            ".");
                    }
                }

                return result;
            };

            return func(parameter);
        }
    }
}
