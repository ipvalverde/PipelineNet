using PipelineNet.Middleware;
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
        public IPipeline<TParameter> Add<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter>
        {
            MiddlewareTypes.Add(typeof(TMiddleware));
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
        public IPipeline<TParameter> Add(Type middlewareType)
        {
            base.AddMiddleware(middlewareType);
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
                MiddlewareResolverResult resolverResult = null;
                try
                {
                    var type = MiddlewareTypes[index];
                    resolverResult = MiddlewareResolver.Resolve(type);
                    var middleware = (IMiddleware<TParameter>)resolverResult.Middleware;

                    index++;
                    if (index == MiddlewareTypes.Count)
                        action = (p) => { };

                    if (resolverResult.IsDisposable && !(middleware is IDisposable))
                    {
#if NETSTANDARD2_1_OR_GREATER
                        if (middleware is IAsyncDisposable)
                        {
                            throw new InvalidOperationException($"'{middleware.GetType().FullName}' type only implements IAsyncDisposable." +
                                " Use AsyncPipeline to execute the configured pipeline.");
                        }
#endif

                        throw new InvalidOperationException($"'{middleware.GetType().FullName}' type does not implement IDisposable.");
                    }

                    middleware.Run(param, action);
                }
                finally
                {
                    if (resolverResult != null && resolverResult.IsDisposable)
                    {
                        var middleware = resolverResult.Middleware;
                        if (middleware != null)
                        {
                            if (middleware is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                }
            };

            action(parameter);
        }
    }
}
