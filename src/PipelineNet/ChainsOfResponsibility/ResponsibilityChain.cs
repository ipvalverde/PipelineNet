using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Collections.Generic;

namespace PipelineNet.ChainsOfResponsibility
{
    public class ResponsibilityChain<TParameter, TReturn> : IResponsibilityChain<TParameter, TReturn>
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;
        private Func<TParameter, TReturn> _finallyFunc;

        public ResponsibilityChain(IMiddlewareResolver middlewareResolver)
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
        /// <param name="finallyFunc">The <see cref="Func{TParameter, TResult}"/> that will be execute at the end of chain.</param>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Finally(Func<TParameter, TReturn> finallyFunc)
        {
            this._finallyFunc = finallyFunc;
            return this;
        }

        /// <summary>
        /// Chain a new middleware to the chain of responsibility.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <typeparam name="TMiddleware">The new middleware being added.</typeparam>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter, TReturn>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Execute the configured chain of responsibility.
        /// </summary>
        /// <param name="parameter"></param>
        public TReturn Execute(TParameter parameter)
        {
            if (_middlewareTypes.Count == 0)
                return default(TReturn);

            int index = 0;
            Func<TParameter, TReturn> func = null;
            func = (param) =>
            {
                var type = _middlewareTypes[index];
                var middleware = (IMiddleware<TParameter, TReturn>)_middlewareResolver.Resolve(type);

                index++;
                // If the current instance of middleware is the last one in the list,
                // the "next" function is assigned to the finally function or a 
                // default empty function.
                if (index == _middlewareTypes.Count)
                    func = this._finallyFunc ?? ((p) => default(TReturn));

                return middleware.Run(param, func);
            };

            return func(parameter);
        }
    }
}
