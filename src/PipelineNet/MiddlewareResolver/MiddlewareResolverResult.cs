namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// Contains the result of <see cref="IMiddlewareResolver"/>.
    /// </summary>
    public class MiddlewareResolverResult
    {
        /// <summary>
        /// The instance of the middleware.
        /// </summary>
        public object Middleware { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the middleware should be disposed.
        /// Set this to <see langword="true"/> if the middleware is IDisposable or
        /// IAsyncDisposable (requires .NET Standard 2.1 or greater)
        /// and is not disposed by a dependency injection container.
        /// </summary>
        public bool IsDisposable { get; set; }
    }
}
