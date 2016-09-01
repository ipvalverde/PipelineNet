using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNet.Middleware
{
    public interface IMiddleware<TParameter, TResult>
    {
        TResult Run(TParameter parameter, Func<TParameter, TResult> next);
    }

    public interface IMiddleware<TParameter>
    {
        void Run(TParameter parameter, Action<TParameter> next);
    }
}
