namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

[ProviderAlias("ZLoggerPlain")]
internal sealed class ZLoggerPlainLoggerProvider(PlainLogProcessor processor, ZLoggerPlainOptions options) : ZLoggerGenericLoggerProvider<PlainLogProcessor, ZLoggerPlainOptions>(processor, options);
