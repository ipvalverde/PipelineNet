using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;

namespace PipelineNet.ChainsOfResponsibility
{
    public class AsyncResponsibilityChain<TParameter, TReturn> : IAsyncResponsibilityChain<TParameter, TReturn>
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;
        private Func<TParameter, Task<TReturn>> _finallyFunc;

        public AsyncResponsibilityChain(IMiddlewareResolver middlewareResolver)
        {
            if (middlewareResolver == null) throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided. You can use ActivatorMiddlewareResolver.");

            _middlewareResolver = middlewareResolver;
            _middlewareTypes = new List<Type>();
        }

        /// <summary>
        /// Sets the function to be executed at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method more
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}<"/>.
        /// </summary>
        /// <param name="finallyFunc">The <see cref="Func{TParameter, Task{TResult}}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IAsyncResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>() where TMiddleware : IAsyncMiddleware<TParameter, TReturn>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Chain a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public async Task<TReturn> Execute(TParameter parameter)
        {
            if (_middlewareTypes.Count == 0)
                return default(TReturn);

            int index = 0;
            Func<TParameter, Task<TReturn>> func = null;
            func = (param) =>
            {
                var type = _middlewareTypes[index];
                var middleware = (IAsyncMiddleware<TParameter, TReturn>)_middlewareResolver.Resolve(type);

                index++;
                // If the current instance of middleware is the last one in the list,
                // the "next" function is assigned to the finally function or a 
                // default empty function.
                if (index == _middlewareTypes.Count)
                    func = this._finallyFunc ?? ((p) => Task.FromResult(default(TReturn)));

                return middleware.Run(param, func);
            };

            return await func(parameter);
        }

        /// <summary>
        /// Execute the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public IAsyncResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, Task<TReturn>> finallyFunc)
        {
            this._finallyFunc = finallyFunc;
            return this;
        }
    }
}
