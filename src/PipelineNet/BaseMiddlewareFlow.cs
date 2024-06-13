using System.Collections.Generic;
using PipelineNet.Middleware;

namespace PipelineNet
{
    public abstract class BaseMiddlewareFlow<TMiddleware>
    {
        public List<TMiddleware> Middleware{get;protected set;} = new List<TMiddleware>();

    }
}