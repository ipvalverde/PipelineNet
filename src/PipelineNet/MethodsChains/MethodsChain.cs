using System;

namespace PipelineNet.MethodsChains
{
    /// <summary>
    /// Methods chain implementation
    /// </summary>
    public class MethodsChain<TInput, TOutput> : IMethodsChain<TInput, TOutput>
    {
        public Func<TInput, TOutput> OnCall { get; set; }

        public MethodsChain(Func<TInput, TOutput> onCall)
        {
            OnCall = onCall;
        }
        public static IMethodsChain<TInput, TOutput> Chain(Func<TInput, TOutput> nextCall) => new MethodsChain<TInput,TOutput>(nextCall);
        public static IMethodsChain<TInput, TOutput> Chain(IMethodsChain<TInput, TOutput> nextCall) => new MethodsChain<TInput,TOutput>(nextCall.Run);
        public IMethodsChain<TInput, TNext> Chain<TNext>(Func<TOutput, TNext> nextCall)
        {
            return new MethodsChain<TInput, TNext>(
                input =>
                {
                    TOutput output;
                    output = Run(input);
                    return nextCall(output);
                }
            );
        }
        public IMethodsChain<TInput, TOutput> Chain(Action<TOutput> nextCall)=>Chain(x=>{
            nextCall(x);
            return x;
        });

        public IMethodsChain<TInput, TNext> Chain<TNext>(IMethodsChain<TOutput, TNext> nextCall)=>Chain(nextCall.Run);
        public TOutput Run(TInput input)=>OnCall(input);
    }
}
