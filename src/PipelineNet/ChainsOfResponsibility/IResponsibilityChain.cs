using PipelineNet.Middleware;

namespace PipelineNet.ChainsOfResponsibility
{
    public interface IResponsibilityChain<TParameter, TReturn>
        where TParameter : class
        where TReturn : class
    {
        /// <summary>
        /// Adds a middleware type to be executed.
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        IResponsibilityChain<TParameter, TReturn> Chain<TMiddleware>()
            where TMiddleware : IMiddleware<TParameter, TReturn>;

        /// <summary>
        /// Execute the configured pipeline.
        /// </summary>
        /// <param name="parameter"></param>
        TReturn Execute(TParameter parameter);
    }
}
