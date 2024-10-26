using PipelineNet.Finally;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public class AsyncResponsibilityChain<TParameter, TReturn> : AsyncBaseMiddlewareFlow<IAsyncMiddleware<TParameter, TReturn>, ICancellableAsyncMiddleware<TParameter, TReturn>>,
        IAsyncResponsibilityChain<TParameter, TReturn>
    {
        /// <summary>
        /// Stores the <see cref="TypeInfo"/> of the finally type.
        /// </summary>
        private static readonly TypeInfo FinallyTypeInfo = typeof(IAsyncFinally<TParameter, TReturn>).GetTypeInfo();

        /// <summary>
        /// Stores the <see cref="TypeInfo"/> of the cancellable finally type.
        /// </summary>
        private static readonly TypeInfo CancellableFinallyTypeInfo = typeof(ICancellableAsyncFinally<TParameter, TReturn>).GetTypeInfo();


        private Type _finallyType;
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
        /// Chains a new cancellable middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TCancellableMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> ChainCancellable<TCancellableMiddleware>() where TCancellableMiddleware : ICancellableAsyncMiddleware<TParameter, TReturn>
        {
            MiddlewareTypes.Add(typeof(TCancellableMiddleware));
            return this;
        }

        /// <summary>
        /// Chains a new middleware type to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IAsyncMiddleware{TParameter, TReturn}"/> or <see cref="ICancellableAsyncMiddleware{TParameter, TReturn}"/>.</exception>
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
        public async Task<TReturn> Execute(TParameter parameter) =>
            await Execute(parameter, default).ConfigureAwait(false);

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken">The cancellation token that will be passed to all middleware.</param>
        public async Task<TReturn> Execute(TParameter parameter, CancellationToken cancellationToken)
        {
            if (MiddlewareTypes.Count == 0)
                return default(TReturn);

            int index = 0;
            Func<TParameter, Task<TReturn>> func = null;
            func = async (param) =>
            {
                MiddlewareResolverResult resolverResult = null;
                MiddlewareResolverResult finallyResolverResult = null;
                try
                {
                    var type = MiddlewareTypes[index];
                    resolverResult = MiddlewareResolver.Resolve(type);

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
                            func = async (p) => await RunFinallyAsync(finallyResolverResult, p, cancellationToken).ConfigureAwait(false);
                        }
                        else if (_finallyFunc != null)
                        {
                            func = _finallyFunc;
                        }
                        else
                        {
                            func = async (p) => await Task.FromResult(default(TReturn)).ConfigureAwait(false);
                        }
                    }

                    EnsureMiddlewareNotNull(resolverResult, type);
                    return await RunMiddlewareAsync(resolverResult, param, func, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await DisposeMiddlewareAsync(resolverResult).ConfigureAwait(false);
                    await DisposeMiddlewareAsync(finallyResolverResult).ConfigureAwait(false);
                }
            };

            return await func(parameter).ConfigureAwait(false);
        }

        private static async Task<TReturn> RunMiddlewareAsync(
            MiddlewareResolverResult middlewareResolverResult,
            TParameter parameter,
            Func<TParameter, Task<TReturn>> next,
            CancellationToken cancellationToken)
        {
            if (middlewareResolverResult.Middleware is ICancellableAsyncMiddleware<TParameter, TReturn> cancellableMiddleware)
            {
                return await cancellableMiddleware.Run(parameter, next, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var middleware = (IAsyncMiddleware<TParameter, TReturn>)middlewareResolverResult.Middleware;
                return await middleware.Run(parameter, next).ConfigureAwait(false);
            }
        }

        private static async Task<TReturn> RunFinallyAsync(
            MiddlewareResolverResult finallyResolverResult,
            TParameter parameter,
            CancellationToken cancellationToken)
        {
            if (finallyResolverResult.Middleware is ICancellableAsyncFinally<TParameter, TReturn> cancellableFinally)
            {
                return await cancellableFinally.Finally(parameter, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var @finally = (IAsyncFinally<TParameter, TReturn>)finallyResolverResult.Middleware;
                return await @finally.Finally(parameter).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally type. Calling this method more
        /// a second time will just replace the existing finally type.
        /// </summary>
        /// <typeparam name="TFinally">The finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Finally<TFinally>()
            where TFinally : IAsyncFinally<TParameter, TReturn>
        {
            _finallyType = typeof(TFinally);
            return this;
        }

        /// <summary>
        /// Sets the cancellable finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally type. Calling this method more
        /// a second time will just replace the existing finally type.
        /// </summary>
        /// <typeparam name="TCancellableFinally">The cancellable finally being set.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> CancellableFinally<TCancellableFinally>()
            where TCancellableFinally : ICancellableAsyncFinally<TParameter, TReturn>
        {
            _finallyType = typeof(TCancellableFinally);
            return this;
        }

        /// <summary>
        /// Sets the finally to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally type. Calling this method more
        /// a second time will just replace the existing finally type.
        /// </summary>
        /// <param name="finallyType">The <see cref="IAsyncFinally{TParameter, TReturn}"/> or <see cref="ICancellableAsyncFinally{TParameter, TReturn}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Finally(Type finallyType)
        {
            if (finallyType == null) throw new ArgumentNullException("finallyType");

            bool isAssignableFromFinally = FinallyTypeInfo.IsAssignableFrom(finallyType.GetTypeInfo())
                || CancellableFinallyTypeInfo.IsAssignableFrom(finallyType.GetTypeInfo());
            if (!isAssignableFromFinally)
                throw new ArgumentException(
                    $"The finally type must implement \"{typeof(IAsyncFinally<TParameter, TReturn>)}\" or \"{typeof(ICancellableAsyncFinally<TParameter, TReturn>)}\".");

            _finallyType = finallyType;
            return this;
        }

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="finallyFunc">The function that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        [Obsolete("This overload is obsolete. Use Finally<TFinally> or CancellableFinally<TCancellableFinally>.")]
        public IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc)
        {
            this._finallyFunc = finallyFunc;
            return this;
        }
    }
}
