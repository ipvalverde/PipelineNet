using PipelineNet.MiddlewareResolver;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PipelineNet
{
    public abstract class BaseMiddlewareFlow<TMiddleware>
    {
        protected IList<Type> MiddlewareTypes { get; private set; }
        protected IMiddlewareResolver MiddlewareResolver { get; private set; }

        internal BaseMiddlewareFlow(IMiddlewareResolver middlewareResolver)
        {
            if (middlewareResolver == null) throw new ArgumentNullException("middlewareResolver",
                "An instance of IMiddlewareResolver must be provided. You can use ActivatorMiddlewareResolver.");

            MiddlewareResolver = middlewareResolver;
            MiddlewareTypes = new List<Type>();
        }

        /// <summary>
        /// Stores the <see cref="TypeInfo"/> of the middleware type.
        /// </summary>
        private static readonly TypeInfo MiddlewareTypeInfo = typeof(TMiddleware).GetTypeInfo();

        /// <summary>
        /// Adds a new middleware type to the internal list of types.
        /// Middleware will be executed in the same order they are added.
        /// </summary>
        /// <param name="middlewareType">The middleware type to be executed.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="middlewareType"/> is 
        /// not an implementation of <typeparamref name="TMiddleware"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middlewareType"/> is null.</exception>
        protected void AddMiddleware(Type middlewareType)
        {
            if (middlewareType == null) throw new ArgumentNullException("middlewareType");

            bool isAssignableFromMiddleware = MiddlewareTypeInfo.IsAssignableFrom(middlewareType.GetTypeInfo());
            if (!isAssignableFromMiddleware)
                throw new ArgumentException(
                    string.Format("The middleware type must implement \"{0}\".", typeof(TMiddleware)));

            this.MiddlewareTypes.Add(middlewareType);
        }
    }
}
