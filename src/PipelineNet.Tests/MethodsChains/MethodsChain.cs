using System;
using PipelineNet.MethodsChains;
using Xunit;

namespace PipelineNet.Tests.MethodsChains{
    public class MethodsChainsTests{
        [Fact]
        public void NumberTransform(){
            var chain = MethodsChain<int,string>
                .Chain(num=>(1+num).ToString())
                .Chain(numStr=>numStr+"2")
                .Chain(int.Parse);
            var expected = 12352;
            var actual = chain.Run(1234);
            Assert.Equal(expected,actual);
        }
        [Fact]
        public void MethodAbortion(){
            bool reachedEnd = false;
            var chain = MethodsChain<int,int>
                .Chain(num=>num+1)
                .Chain(num=>num % 3 == 0 ? num/3 - 1 : throw new Exception())
                .Chain(num=>{
                    reachedEnd = true;
                    return num+1 % 2 == 0 ? "yes" : "no";
                });
            Assert.Throws<Exception>(()=>chain.Run(25));
            Assert.False(reachedEnd);
        }
    }
}