using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineNet.ChainsOfResponsibility
{
    /// <summary>
    /// Defines the asynchronous chain of responsibility.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the chain.</typeparam>
    /// <typeparam name="TReturn">The return type of the chain.</typeparam>
    public class AsyncResponsibilityChain<TParameter, TReturn> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter,TReturn>>, IAsyncResponsibilityChain<TParameter, TReturn>
    {
        private Func<TParameter, Task<TReturn>> _finallyFunc;

        /// <summary>
        /// Chains a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>(Action<TMiddleware> configure = null) where TMiddleware : IAsyncMiddleware<TParameter, TReturn>, new()
        {
            var mw = new TMiddleware();
            configure?.Invoke(mw);
            Middleware.Add(mw);
            return this;
        }

        /// <summary>
        /// Executes the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public async Task<TReturn> Execute(TParameter parameter)
        {
            var res = await Do(parameter,Middleware.GetEnumerator());
            return res;

            async Task<TReturn> Do(TParameter _param,IEnumerator<IAsyncMiddleware<TParameter,TReturn>> e){
                
                if(!e.MoveNext()){
                    if(!(_finallyFunc is null))
                        return await _finallyFunc?.Invoke(_param);
                    return default;
                }
                return await e.Current.Run(_param,async p=>{
                    return await Do(p,e);
                });
            }
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
