﻿using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// A pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public class Pipeline<TParameter> : BaseMiddlewareFlow<IMiddleware<TParameter>>, IPipeline<TParameter>
    {
        /// <summary>
        /// Creates a new instance of Pipeline.
        /// </summary>
        /// <param name="middlewareResolver">Resolver responsible for resolving instances out of middleware types.</param>
        public Pipeline(IMiddlewareResolver middlewareResolver) : base(middlewareResolver)
        {}

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public IPipeline<TParameter> Add<TMiddleware>(object args = null)
            where TMiddleware : IMiddleware<TParameter>
        {
            MiddlewareTypes.Add(new Tuple<Type,object>(typeof(TMiddleware),args));
            return this;
        }

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <see cref="IMiddleware{TParameter}"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        public IPipeline<TParameter> Add(Type middlewareType,object args = null)
        {
            base.AddMiddleware(middlewareType,args);
            return this;
        }

        /// <summary>
        /// Executes the configured pipeline.
        /// </summary>
        /// <param name="parameter">The input that will be provided to all middleware.</param>
        public void Execute(TParameter parameter)
        {
            if (MiddlewareTypes.Count == 0)
                return;

            int index = 0;
            Action<TParameter> action = null;
            action = (param) =>
            {
                var type = MiddlewareTypes[index];
                var middleware = (IMiddleware<TParameter>)MiddlewareResolver.Resolve(type.Item1);

                index++;
                if (index == MiddlewareTypes.Count)
                    action = (p) => { };

                middleware.Run(param, action,type.Item2);
            };

            action(parameter);
        }
    }
}