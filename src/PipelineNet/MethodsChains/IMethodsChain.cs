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
        IMethodsChain<TInput, TNext> Chain<TNext>(Func<TOutput, TNext> nextCall);
        /// <summary>
        /// Chains another chain
        /// </summary>
        IMethodsChain<TInput, TNext> Chain<TNext>(IMethodsChain<TOutput, TNext> nextCall);
        /// <summary>
        /// Chains current chain without output - just will pass input to next chain
        /// </summary>
        /// <param name="nextCall"></param>
        /// <returns></returns>
        IMethodsChain<TInput,TOutput> Chain(Action<TOutput> nextCall);
        /// <summary>
        /// Use to get output from input.<br/>
        /// </summary>
        TOutput Run(TInput input);
    }
}
