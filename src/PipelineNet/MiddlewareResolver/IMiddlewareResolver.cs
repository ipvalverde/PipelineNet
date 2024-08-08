using System;

namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// Used to resolve instances of middleware.
    /// You can implement this interface for your preferred dependency injection container.
    /// </summary>
    public interface IMiddlewareResolver
    {
        /// <summary>
        /// Resolves an instance of the given middleware type.
        /// </summary>
        /// <param name="type">The middleware type that will be resolved.</param>
        /// <returns>An instance of the middleware.</returns>
        /// <remarks>If the instance of the given middleware type could not be resolved, the exception should be thrown rather than returning null or partial result.</remarks>
        MiddlewareResolverResult Resolve(Type type);
    }
}
