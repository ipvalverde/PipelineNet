using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Collections.Generic;

namespace PipelineNet.Pipelines
{
    public class Pipeline<TParameter> : IPipeline<TParameter>
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;

        public Pipeline(IMiddlewareResolver middlewareResolver)
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
        public IPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        public void Execute(TParameter parameter)
        {
            if (_middlewareTypes.Count == 0)
                return;

            int index = 0;
            Action<TParameter> action = null;
            action = (param) =>
            {
                var type = _middlewareTypes[index];
                var middleware = (IMiddleware<TParameter>)_middlewareResolver.Resolve(type);

                index++;
                if (index == _middlewareTypes.Count)
                    action = (p) => { };

                middleware.Run(param, action);
            };

            action(parameter);
        }
    }
}