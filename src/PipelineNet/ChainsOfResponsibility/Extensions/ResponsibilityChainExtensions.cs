using System;

namespace PipelineNet.ChainsOfResponsibility.Extensions
{
    /// <summary>
    /// Extension methods to the chain of responsibility.
    /// </summary>
    public static class ResponsibilityChainExtensions
    {
        /// <summary>
        /// Throws the <see cref="InvalidOperationException"/> at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="chain">The instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</param>
        /// <returns>The current instance of <see cref="IResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public static IResponsibilityChain<TParameter, TReturn> FinallyThrow<TParameter, TReturn>(this IResponsibilityChain<TParameter, TReturn> chain)
        {
            if (chain == null) throw new ArgumentNullException("chain");

            return chain.Finally(_ => throw new InvalidOperationException("End of the chain of responsibility reached. No middleware matches returned a value."));
        }
    }
}
