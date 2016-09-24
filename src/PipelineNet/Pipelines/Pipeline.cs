using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// A pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public class Pipeline<TParameter> : IPipeline<TParameter>
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;

        /// <summary>
        /// Creates a new instance of Pipeline.
        /// </summary>
        /// <param name="middlewareResolver">Resolver responsible for resolving instances out of middleware types.</param>
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

        private static readonly TypeInfo MiddlewareTypeInfo = typeof(IMiddleware<TParameter>).GetTypeInfo();

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middleType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/>.</exception>
        public IPipeline<TParameter> Add(Type middlewareType)
        {
            if (middlewareType == null) throw new ArgumentNullException("middlewareType");

            bool isAssignableFromMiddleware = MiddlewareTypeInfo.IsAssignableFrom(middlewareType.GetTypeInfo());
            if (!isAssignableFromMiddleware)
                throw new ArgumentException("The middleware type must implement IMiddleware.");

            this._middlewareTypes.Add(middlewareType);
            return this;
        }

        /// <summary>
        /// Executes the configured pipeline.
        /// </summary>
        /// <param name="parameter">The input that will be provided to all middleware.</param>
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