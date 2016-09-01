using PipelineNet.Middleware;
using PipelineNet.ServiceDependency;
using System;
using System.Collections.Generic;

namespace PipelineNet
{
    public class Pipeline<TParameter, TResult>
    {
        private readonly IList<Type> _middlewareTypes;
        private readonly IMiddlewareResolver _middlewareResolver;

        public Pipeline(IMiddlewareResolver middlewareResolver)
        {
            if (middlewareResolver == null) throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided. You can try to use ActivatorMiddlewareResolver.");

            _middlewareResolver = middlewareResolver;
            _middlewareTypes = new List<Type>();
        }

        public void Add<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter, TResult>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
        }

        public TResult Execute(TParameter parameter)
        {
            // ToDo create instances of the types in the list
            // ToDo execute the middleware

            throw new NotImplementedException();
        }
    }
}
