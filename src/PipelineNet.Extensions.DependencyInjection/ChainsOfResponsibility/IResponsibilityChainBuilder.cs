namespace PipelineNet.Extensions.DependencyInjection.Pipelines
{
    /// <summary>
    /// A builder for configuring chain of responsibility instances.
    /// </summary>
    public interface IResponsibilityChainBuilder<TParameter, TReturn>
    {
        /// <summary>
        /// Configures a binding between the <typeparamref name="TClient"/> type and the chain of responsibility
        /// associated with this builder. The created instances will be of type <typeparamref name="TImplementation"/>.
        /// </summary>
        IResponsibilityChainBuilder<TParameter, TReturn> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient;
    }
}
