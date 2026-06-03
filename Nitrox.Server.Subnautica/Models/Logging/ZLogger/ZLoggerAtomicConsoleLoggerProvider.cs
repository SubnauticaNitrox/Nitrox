using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using ZLogger.Providers;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

/// <summary>
///     Writes logs to console with optional atomic logging to ensure related logs are shown together.
/// </summary>
[ProviderAlias("ZLoggerAtomicConsole")]
internal sealed class ZLoggerAtomicConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope, IAsyncDisposable
{
    private readonly ZLoggerConsoleOptions options;
    private readonly IAsyncLogProcessor processor;
    private IExternalScopeProvider? scopeProvider;

    public ZLoggerAtomicConsoleLoggerProvider(ZLoggerConsoleOptions options)
    {
        this.options = options;
        if (options.LogToStandardErrorThreshold == LogLevel.None)
        {
            processor = new AtomicAsyncLogProcessor(new AsyncStreamLineMessageWriter(Console.OpenStandardOutput(), this.options));
        }
        else
        {
            LogLevel logToStandardErrorThreshold = options.LogToStandardErrorThreshold;
            processor = new DualAsyncLogProcessor(new AtomicAsyncLogProcessor(new AsyncStreamLineMessageWriter(Console.OpenStandardOutput(), this.options)),
                                                  new AtomicAsyncLogProcessor(new AsyncStreamLineMessageWriter(Console.OpenStandardError(), this.options)),
                                                  (Func<LogLevel, bool>)(level => level < logToStandardErrorThreshold));
        }
    }

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

    private sealed class DualAsyncLogProcessor(AtomicAsyncLogProcessor processor1, AtomicAsyncLogProcessor processor2, Func<LogLevel, bool> levelFilter) : IAsyncLogProcessor
    {
        public void Post(IZLoggerEntry log)
        {
            IAsyncLogProcessor selectedProcessor = levelFilter(log.LogInfo.LogLevel) ? processor1 : processor2;
            selectedProcessor.Post(log);
        }

        public async ValueTask DisposeAsync()
        {
            ValueTask valueTask = processor1.DisposeAsync();
            ValueTask t2 = processor2.DisposeAsync();
            await valueTask;
            await t2;
            t2 = new ValueTask();
        }
    }

    private sealed record AtomicAsyncLogProcessor(AsyncStreamLineMessageWriter Processor) : IAsyncLogProcessor
    {
        private readonly SemaphoreSlim postLocker = new(1, 1);

        public async ValueTask DisposeAsync()
        {
            await Processor.DisposeAsync().ConfigureAwait(false);
        }

        public void Post(IZLoggerEntry log)
        {
            if (log.TryGetProperty(out AtomicScope scope))
            {
                scope.Locker = postLocker;
                scope.AddLogEntry(log, Processor);
                return;
            }
            try
            {
                while (!postLocker.Wait(TimeSpan.FromMicroseconds(100)))
                {
                }
                Processor.Post(log);
            }
            finally
            {
                postLocker.Release();
            }
        }
    }
}
