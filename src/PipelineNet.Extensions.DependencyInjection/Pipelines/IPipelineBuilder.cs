namespace PipelineNet.Extensions.DependencyInjection.Pipelines
{
    /// <summary>
    /// A builder for configuring pipeline instances.
    /// </summary>
    public interface IPipelineBuilder<TParameter>
    {
        /// <summary>
        /// Configures a binding between the <typeparamref name="TClient"/> type and the pipeline
        /// associated with this builder. The created instances will be of type <typeparamref name="TImplementation"/>.
        /// </summary>
        IPipelineBuilder<TParameter> AddTypedClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient;
    }
}
