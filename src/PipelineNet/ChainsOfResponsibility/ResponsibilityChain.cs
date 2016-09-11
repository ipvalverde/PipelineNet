using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Collections.Generic;

namespace PipelineNet.ChainsOfResponsibility
{
    public class ResponsibilityChain<TParameter, TReturn> : IResponsibilityChain<TParameter, TReturn>
        where TParameter : class
        where TReturn : class
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;

        public ResponsibilityChain(IMiddlewareResolver middlewareResolver)
        {
            if (middlewareResolver == null) throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided. You can use ActivatorMiddlewareResolver.");

            _middlewareResolver = middlewareResolver;
            _middlewareTypes = new List<Type>();
        }

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter, TReturn>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

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
                if (index == _middlewareTypes.Count)
                    func = (p) => default(TReturn);

                return middleware.Run(param, func);
            };

            return func(parameter);
        }
    }
}
