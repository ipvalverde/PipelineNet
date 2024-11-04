using System;

namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// A default implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the <see cref="System.Activator"/>.
    /// </summary>
    public class ActivatorMiddlewareResolver : IMiddlewareResolver
    {
        /// <inheritdoc/>
        public MiddlewareResolverResult Resolve(Type type)
        {
            var middleware = Activator.CreateInstance(type);
            bool dispose = true;

            return new MiddlewareResolverResult()
            {
                Middleware = middleware,
                Dispose = dispose
            };
        }
    }
}
