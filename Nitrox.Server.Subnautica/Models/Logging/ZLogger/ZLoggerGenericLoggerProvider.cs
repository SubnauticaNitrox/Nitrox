namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal abstract class ZLoggerGenericLoggerProvider<TProcessor, TOptions>(
    TProcessor processor,
    TOptions options) :
    ILoggerProvider,
    ISupportExternalScope,
    IAsyncDisposable
    where TProcessor : IAsyncLogProcessor
    where TOptions : ZLoggerOptions
{
    private readonly TOptions options = options;
    private readonly TProcessor processor = processor;
    private IExternalScopeProvider? scopeProvider;

    public ILogger CreateLogger(string categoryName)
    {
        return new ZLoggerLogger(categoryName, processor, options, options.IncludeScopes ? scopeProvider : null);
    }

    public void Dispose() => processor.DisposeAsync().AsTask().Wait();

    public async ValueTask DisposeAsync()
    {
        await processor.DisposeAsync().ConfigureAwait(false);
    }

    public void SetScopeProvider(IExternalScopeProvider provider)
    {
        scopeProvider = provider;
    }
}
