using PipelineNet.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNet.Pipelines
{
    /// <summary>
    /// A pipeline stores middleware that are executed when <see cref="Execute(TParameter)"/> is called.
    /// The middleware are executed in the same order they are added.
    /// </summary>
    /// <typeparam name="TParameter">The type that will be the input for all the middleware.</typeparam>
    public class Pipeline<TParameter> : BaseMiddlewareFlow<IMiddleware<TParameter>>, IPipeline<TParameter>
    {

        public Pipeline()
        {

        }
         /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public IPipeline<TParameter> Add<TMiddleware>(Action<TMiddleware> configure = null)
            where TMiddleware : IMiddleware<TParameter>,new()
        {

            var middleware = new TMiddleware();
            if (middleware==null){

            }
            configure?.Invoke(middleware);
            Middleware.Add(middleware);

            return this;
        }

        /// <summary>
        /// Executes the configured pipeline.
        /// </summary>
        /// <param name="parameter">The input that will be provided to all middleware.</param>
        public void Execute(TParameter parameter)
        {

            Do(parameter,Middleware.GetEnumerator());
            return;
            
            void Do(TParameter _param,IEnumerator<IMiddleware<TParameter>> e){
                if(!e.MoveNext()) return;
                e.Current.Run(_param,p=>{
                    Do(p,e);
                });
            }
        }
    }
}