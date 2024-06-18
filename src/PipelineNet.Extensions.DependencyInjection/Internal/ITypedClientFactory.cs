namespace PipelineNet.Extensions.DependencyInjection.Internal
{
    internal interface ITypedClientFactory<TClient, TMiddlewareFlow>
    {
        TClient CreateClient(TMiddlewareFlow middlewareFlow);
    }
}