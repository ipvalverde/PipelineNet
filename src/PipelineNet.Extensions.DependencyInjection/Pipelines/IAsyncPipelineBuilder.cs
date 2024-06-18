namespace PipelineNet.Extensions.DependencyInjection.Pipelines
{
    /// <summary>
    /// A builder for configuring asynchronous pipeline instances.
    /// </summary>
    public interface IAsyncPipelineBuilder<TParameter>
    {
        /// <summary>
        /// Configures a binding between the <typeparamref name="TClient"/> type and the asynchronous pipeline
        /// associated with this builder. The created instances will be of type <typeparamref name="TImplementation"/>.
        /// </summary>
        IAsyncPipelineBuilder<TParameter> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient;
    }
}
