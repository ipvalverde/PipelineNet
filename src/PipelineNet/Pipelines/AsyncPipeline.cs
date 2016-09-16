using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public class AsyncPipeline<TParameter> : IAsyncPipeline<TParameter>
        where TParameter : class
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;

        public AsyncPipeline(IMiddlewareResolver middlewareResolver)
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
        public IAsyncPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IAsyncMiddleware<TParameter>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        public async Task Execute(TParameter parameter)
        {
            if (_middlewareTypes.Count == 0)
                return;

            int index = 0;
            Func<TParameter, Task> action = null;
            action = async (param) =>
            {
                var type = _middlewareTypes[index];
                var firstMiddleware = (IAsyncMiddleware<TParameter>)_middlewareResolver.Resolve(type);

                index++;
                if (index == _middlewareTypes.Count)
                    action = async (p) => { };

                await firstMiddleware.Run(param, action);
            };

            await action(parameter);
        }
    }
}
