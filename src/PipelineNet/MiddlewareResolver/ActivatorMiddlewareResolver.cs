using System;

namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// A default implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the <see cref="Activator"/>.
    /// </summary>
    public class ActivatorMiddlewareResolver : IMiddlewareResolver
    {
        /// <inheritdoc/>
        public MiddlewareResolverResult Resolve(Type type)
        {
            var middleware = Activator.CreateInstance(type);
            bool isDisposable = middleware is IDisposable
#if NETSTANDARD2_1_OR_GREATER
                || middleware is IAsyncDisposable
#endif
                ;

            return new MiddlewareResolverResult()
            {
                Middleware = middleware,
                IsDisposable = isDisposable
            };
        }
    }
}
