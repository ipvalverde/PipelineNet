using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public class ResponsibilityChain<TParameter, TReturn> : BaseMiddlewareFlow<IMiddleware<TParameter, TReturn>>, IResponsibilityChain<TParameter, TReturn>
    {
        private Func<TParameter, TReturn> _finallyFunc;

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
        public IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IMiddleware<TParameter, TReturn>, new()
        {
            var mw = new TMiddleware();
            configure?.Invoke(mw);
            Middleware.Add(mw);
            return this;
        }

        public IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>([NotNull] TMiddleware middleware) where TMiddleware : IMiddleware<TParameter, TReturn>, new()
        {
            Middleware.Add(middleware);
            return this;
        }

        /// <summary>
        /// Execute the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public TReturn Execute(TParameter parameter)
        {
            var res = Do(parameter, Middleware.GetEnumerator());
            return res;

            TReturn Do(TParameter _param, IEnumerator<IMiddleware<TParameter, TReturn>> e)
            {
                if (!e.MoveNext())
                {
                    if (_finallyFunc is null)
                        return default(TReturn);
                    else
                        return _finallyFunc.Invoke(_param);
                }

                return e.Current.Run(_param, p =>
                {
                    return Do(p, e);
                });
            }
        }

    }
}
