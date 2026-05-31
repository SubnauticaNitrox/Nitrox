using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Logging.ZLogger;
using Nitrox.Server.Subnautica.Services;
using ZLogger.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class LoggingBuilderExtensions
{
    extension(ILoggingBuilder builder)
    {
        private ILoggingBuilder AddNitroxZLoggerPlain(Action<ZLoggerPlainOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, ZLoggerPlainLoggerProvider>(_ =>
            {
                PlainLogProcessor processor = new() { Options = new() };
                configure(processor.Options);
                processor.Formatter = processor.Options.CreateFormatter();
                return new ZLoggerPlainLoggerProvider(processor, processor.Options);
            });
            return builder;
        }

        public ILoggingBuilder AddNitroxLogging()
        {
            builder.Services.AddRedactors();
            return builder
                   .AddZLoggerConsole(static (options, provider) =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(formatterOptions =>
                       {
                           formatterOptions.OmitWhenCaptured = true;
                           bool isEmbedded = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded;
                           formatterOptions.ColorBehavior = isEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                       });
                   })
                   .AddNitroxZLoggerPlain(options =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(o =>
                       {
                           o.OmitWhenCaptured = true;
                           o.IsPlain = true;
                       }).OutputFunc = async (entry, formatter, generator, writer) => await ServersManagementService.LogQueue.Writer.WriteAsync(new ServersManagementService.LogEntry(entry, formatter, generator, writer));
                   })
                   .AddNitroxZLoggerPlain(options =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter().OutputFunc = (entry, formatter, generator, writer) =>
                       {
                           if (entry.TryGetProperty(out CaptureScope scope))
                           {
                               scope.Capture(generator(entry, formatter, writer));
                           }
                           return Task.CompletedTask;
                       };
                   })
                   .AddZLoggerRollingFile(static (options, provider) =>
                   {
                       ServerStartOptions serverStartOptions = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value;
                       options.FilePathSelector = (timestamp, sequence) =>
                       {
                           string filename = $"{timestamp.ToLocalTime():yyyy-MM-dd}_server_{serverStartOptions.SaveName}_{sequence:000}.log";
                           return Path.Combine(serverStartOptions.GetServerLogsPath(), filename);
                       };
                       options.RollingInterval = RollingInterval.Day;
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(formatterOptions =>
                       {
                           formatterOptions.OmitWhenCaptured = true;
                           formatterOptions.Redactors = provider.GetRequiredService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                       });
                   });
        }
    }
}
