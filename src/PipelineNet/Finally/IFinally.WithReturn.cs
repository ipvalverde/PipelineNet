namespace PipelineNet.Finally
{
    /// <summary>
    /// Defines the chain of responsibility finally.
    /// Finally will be executed at the end of the chain as a fallback.
    /// </summary>
    /// <typeparam name="TParameter">The input type for the finally.</typeparam>
    /// <typeparam name="TReturn">The return type of the finally.</typeparam>
    public interface IFinally<TParameter, TReturn>
    {
        /// <summary>
        /// Executes the finally.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        TReturn Finally(TParameter parameter);
    }
}
