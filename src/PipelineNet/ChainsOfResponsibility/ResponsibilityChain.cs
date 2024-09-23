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
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally.
        /// </summary>
        /// <typeparam name="TFinally">The finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Finally<TFinally>()
            where TFinally : IFinally<TParameter, TReturn> =>
            Finally(typeof(TFinally));

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally.
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
                return default(TReturn);

            int index = 0;
            Func<TParameter, TReturn> func = null;
            func = (param) =>
            {
                MiddlewareResolverResult resolverResult = null;
                MiddlewareResolverResult finallyResolverResult = null;
                try
                {
                    var type = MiddlewareTypes[index];
                    resolverResult = MiddlewareResolver.Resolve(type);
                    var middleware = (IMiddleware<TParameter, TReturn>)resolverResult.Middleware;

                    index++;
                    // If the current instance of middleware is the last one in the list,
                    // the "next" function is assigned to the finally function or a 
                    // default empty function.
                    if (index == MiddlewareTypes.Count)
                    {
                        if (_finallyType != null)
                        {
                            finallyResolverResult = MiddlewareResolver.Resolve(_finallyType);

                            if (finallyResolverResult == null || finallyResolverResult.Middleware == null)
                            {
                                throw new InvalidOperationException($"'{MiddlewareResolver.GetType()}' failed to resolve finally of type '{_finallyType}'.");
                            }

                            if (finallyResolverResult.IsDisposable && !(finallyResolverResult.Middleware is IDisposable))
                            {
#if NETSTANDARD2_1_OR_GREATER
                                if (finallyResolverResult.Middleware is IAsyncDisposable)
                                {
                                    throw new InvalidOperationException($"'{finallyResolverResult.Middleware.GetType()}' type only implements IAsyncDisposable." +
                                        " Use AsyncResponsibilityChain to execute the configured pipeline.");
                                }
#endif

                                throw new InvalidOperationException($"'{finallyResolverResult.Middleware.GetType()}' type does not implement IDisposable.");
                            }

                            var @finally = (IFinally<TParameter, TReturn>)finallyResolverResult.Middleware;
                            func = (p) => @finally.Finally(p);
                        }
                        else if (_finallyFunc != null)
                        {
                            func = _finallyFunc;
                        }
                        else
                        {
                            func = (p) => default(TReturn);
                        }
                    }

                    if (resolverResult.IsDisposable && !(middleware is IDisposable))
                    {
#if NETSTANDARD2_1_OR_GREATER
                        if (middleware is IAsyncDisposable)
                        {
                            throw new InvalidOperationException($"'{middleware.GetType().FullName}' type only implements IAsyncDisposable." +
                                " Use AsyncResponsibilityChain to execute the configured pipeline.");
                        }
#endif

                        throw new InvalidOperationException($"'{middleware.GetType().FullName}' type does not implement IDisposable.");
                    }

                    return middleware.Run(param, func);
                }
                finally
                {
                    if (resolverResult != null && resolverResult.IsDisposable)
                    {
                        var middleware = resolverResult.Middleware;
                        if (middleware != null)
                        {
                            if (middleware is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }

                    if (finallyResolverResult != null && finallyResolverResult.IsDisposable)
                    {
                        var @finally = finallyResolverResult.Middleware;
                        if (@finally != null)
                        {
                            if (@finally is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                }
            };

            return func(parameter);
        }
    }
}
