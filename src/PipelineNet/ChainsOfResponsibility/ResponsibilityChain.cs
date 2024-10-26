using PipelineNet.Finally;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Reflection;

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
        /// <summary>
        /// Stores the <see cref="TypeInfo"/> of the finally type.
        /// </summary>
        private static readonly TypeInfo FinallyTypeInfo = typeof(IFinally<TParameter, TReturn>).GetTypeInfo();

        private Type _finallyType;
        private Func<TParameter, TReturn> _finallyFunc;

        /// <summary>
        /// Creates a new chain of responsibility.
        /// </summary>
        /// <param name="middlewareResolver"></param>
        public ResponsibilityChain(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {
        }

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally type. Calling this method more
        /// a second time will just replace the existing finally type.
        /// </summary>
        /// <typeparam name="TFinally">The finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Finally<TFinally>()
            where TFinally : IFinally<TParameter, TReturn>
        {
            _finallyType = typeof(TFinally);
            return this;
        }

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally type. Calling this method more
        /// a second time will just replace the existing finally type.
        /// </summary>
        /// <param name="finallyType">The <see cref="IFinally{TParameter, TReturn}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Finally(Type finallyType)
        {
            if (finallyType == null) throw new ArgumentNullException("finallyType");

            bool isAssignableFromFinally = FinallyTypeInfo.IsAssignableFrom(finallyType.GetTypeInfo());
            if (!isAssignableFromFinally)
                throw new ArgumentException(
                    $"The finally type must implement \"{typeof(IFinally<TParameter, TReturn>)}\".");

            _finallyType = finallyType;
            return this;
        }

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="finallyFunc">The <see cref="Func{TParameter, TResult}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        [Obsolete("This overload is obsolete. Use Finally<TFinally>.")]
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
            {
                MiddlewareResolverResult finallyResolverResult = null;
                try
                {
                    if (_finallyType != null)
                    {
                        finallyResolverResult = MiddlewareResolver.Resolve(_finallyType);
                        EnsureMiddlewareNotNull(finallyResolverResult, _finallyType);
                        return RunFinally(finallyResolverResult, parameter);
                    }
                    else if (_finallyFunc != null)
                    {
                        return _finallyFunc(parameter);
                    }
                    else
                    {
                        return default(TReturn);
                    }
                }
                finally
                {
                    DisposeMiddleware(finallyResolverResult);
                }
            }

            int index = 0;
            Func<TParameter, TReturn> next = null;
            next = (parameter2) =>
            {
                MiddlewareResolverResult middlewareResolverResult = null;
                MiddlewareResolverResult finallyResolverResult = null;
                try
                {
                    var middlewaretype = MiddlewareTypes[index];
                    middlewareResolverResult = MiddlewareResolver.Resolve(middlewaretype);

                    index++;
                    // If the current instance of middleware is the last one in the list,
                    // the "next" function is assigned to the finally function or a 
                    // default empty function.
                    if (index == MiddlewareTypes.Count)
                    {
                        if (_finallyType != null)
                        {
                            finallyResolverResult = MiddlewareResolver.Resolve(_finallyType);
                            EnsureMiddlewareNotNull(finallyResolverResult, _finallyType);
                            next = (p) => RunFinally(finallyResolverResult, p);
                        }
                        else if (_finallyFunc != null)
                        {
                            next = _finallyFunc;
                        }
                        else
                        {
                            next = (p) => default(TReturn);
                        }
                    }

                    EnsureMiddlewareNotNull(middlewareResolverResult, middlewaretype);
                    return RunMiddleware(middlewareResolverResult, parameter2, next);
                }
                finally
                {
                    DisposeMiddleware(middlewareResolverResult);
                    DisposeMiddleware(finallyResolverResult);
                }
            };

            return next(parameter);
        }

        private static TReturn RunMiddleware(MiddlewareResolverResult middlewareResolverResult, TParameter parameter, Func<TParameter, TReturn> next)
        {
            var middleware = (IMiddleware<TParameter, TReturn>)middlewareResolverResult.Middleware;
            return middleware.Run(parameter, next);
        }

        private static TReturn RunFinally(MiddlewareResolverResult finallyResolverResult, TParameter parameter)
        {
            var @finally = (IFinally<TParameter, TReturn>)finallyResolverResult.Middleware;
            return @finally.Finally(parameter);
        }
    }
}
