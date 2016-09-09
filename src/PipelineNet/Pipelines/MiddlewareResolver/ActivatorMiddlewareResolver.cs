using PipelineNet.Pipelines.ServiceDependency;
using System;

namespace PipelineNet.Pipelines.DependencyResolver
{
    /// <summary>
    /// A default implementation of <see cref="IMiddlewareResolver"/> that creates
    /// instances using the regular <see cref="System.Activator"/>.
    /// </summary>
    public class ActivatorMiddlewareResolver : IMiddlewareResolver
    {
        public object Resolve(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
