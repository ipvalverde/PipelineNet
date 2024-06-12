using System;

namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// Used to create instances of middleware.
    /// You can implement this interface for your preferred dependency injection container.
    /// </summary>
    public interface IMiddlewareResolver
    {
        /// <summary>
        /// Creates an instance of the give middleware type.
        /// </summary>
        /// <param name="type">The middleware type that will be created.</param>
        /// <returns>An instance of the middleware.</returns>
        MiddlewareResolverResult Resolve(Type type);
    }
}
