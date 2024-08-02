using System;

namespace PipelineNet.ChainsOfResponsibility.Extensions
{
    /// <summary>
    /// Extension methods to the asynchronous chain of responsibility.
    /// </summary>
    public static class AsyncResponsibilityChainExtensions
    {
        /// <summary>
        /// Throws the <see cref="InvalidOperationException"/> at the end of the chain as a fallback.
        /// A chain can only have one finally function. Calling this method
        /// a second time will just replace the existing finally <see cref="Func{TParameter, TResult}"/>.
        /// </summary>
        /// <param name="chain">The instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</param>
        /// <returns>The current instance of <see cref="IAsyncResponsibilityChain{TParameter, TReturn}"/>.</returns>
        public static IAsyncResponsibilityChain<TParameter, TReturn> FinallyThrow<TParameter, TReturn>(this IAsyncResponsibilityChain<TParameter, TReturn> chain)
        {
            if (chain == null) throw new ArgumentNullException("chain");

            return chain.Finally(_ => throw new InvalidOperationException("End of the asynchronous chain of responsibility reached. No middleware matches returned a value."));
        }
    }
}
