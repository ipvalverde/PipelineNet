using System;

namespace PipelineNet.MethodsChains
{
    /// <summary>
    /// Pipeline-like chain of functions, thats allows continuous data transformation and
    /// support execution abortion
    /// </summary>
    public interface IMethodsChain<TInput, TOutput>
    {
        /// <summary>
        /// Used to get output from input.<br/>
        /// Throw any exception in this method -> it will stop pipeline execution
        /// </summary>
        Func<TInput, TOutput> OnCall { get; }
        /// <summary>
        /// Chains current chain output to next chain input. In the end works like function decorator.
        /// </summary>
        IMethodsChain<TInput, TNext> Chain<TNext>(Func<TOutput, TNext> onCall);
        /// <summary>
        /// Use to get output from input.<br/>
        /// </summary>
        TOutput Run(TInput input);
    }
}
