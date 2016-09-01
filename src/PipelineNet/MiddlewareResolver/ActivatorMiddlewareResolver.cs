using PipelineNet.ServiceDependency;
using System;

namespace PipelineNet.DependencyResolver
{
    /// <summary>
    /// A default implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the regular <see cref="System.Activator"/>.
    /// </summary>
    public class ActivatorDependencyResolver : IMiddlewareResolver
    {
        public object Resolve(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
