using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    public class AsyncPipeline<TParameter> : BaseMiddlewareFlow<IAsyncMiddleware<TParameter>>, IAsyncPipeline<TParameter>
        where TParameter : class
    {

        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public IAsyncPipeline<TParameter> Add<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IAsyncMiddleware<TParameter>, new()
        {
            var middleware = new TMiddleware();
            configure?.Invoke(middleware);
            Middleware.Add(middleware);
            return this;
        }


        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        public async Task Execute(TParameter parameter)
        {
            await Do(parameter,Middleware.GetEnumerator());

            async Task Do(TParameter _param,IEnumerator<IAsyncMiddleware<TParameter>> e){
                if(!e.MoveNext()) return;
                await e.Current.Run(_param,async p=>{
                    await Do(p,e);
                });
            }
        }
    }
}
