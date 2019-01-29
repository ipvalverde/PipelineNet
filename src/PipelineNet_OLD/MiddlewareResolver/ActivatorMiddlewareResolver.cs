using System;

namespace PipelineNet.MiddlewareResolver
{
    /// <summary>
    /// A default implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the <see cref="System.Activator"/>.
    /// </summary>
    public class ActivatorMiddlewareResolver : IMiddlewareResolver
    {
        public object Resolve(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
