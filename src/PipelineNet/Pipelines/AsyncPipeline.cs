using PipelineNet.Pipelines.ServiceDependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    action = (p) => { return Task.FromResult(0); };

                await firstMiddleware.Run(param, action);
            };

            await action(parameter);
        }
    }
}
