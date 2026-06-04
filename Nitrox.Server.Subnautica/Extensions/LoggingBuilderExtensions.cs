using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Model.Core;
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
        public ILoggingBuilder AddNitroxLogging()
        {
            builder.Services.AddRedactors();
            return builder
                   .AddNitroxAtomicZLoggerConsole(static (options, provider) =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(provider, o =>
                       {
                           o.IsOmittedOnCapture = true;
                           bool isEmbedded = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded;
                           o.ColorBehavior = isEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                       });
                   })
                   .AddNitroxZLoggerPlain(static (options, provider) =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(provider, o =>
                       {
                           o.IsOmittedOnCapture = true;
                           o.IsPlain = true;
                       }).OutputFunc = async (entry, formatter, generator, writer) => await ServersManagementService.LogQueue.Writer.WriteAsync(new ServersManagementService.LogEntry(entry, formatter, generator, writer));
                   })
                   .AddNitroxZLoggerPlain(static (options, provider) =>
                   {
                       options.IncludeScopes = true;
                       options.UseNitroxFormatter(provider, o =>
                       {
                           o.RequiredPropertyTypes = [typeof(CaptureScope)];
                           o.Redactors = provider.GetRequiredService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                       }).OutputFunc = (entry, formatter, generator, writer) =>
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
                       options.UseNitroxFormatter(provider, o =>
                       {
                           o.HeaderFactory = provider =>
                           {
                               const int LEFT_AND_RIGHT_PADDING = 4;
                               const int PADDING_SPACING = 1;
                               const char PADDING_CHAR = '=';
                               string headerLine = $"{NitroxEnvironment.AppName} {NitroxEnvironment.Version} {NitroxEnvironment.GitHash}";
                               headerLine = $"{new string(' ', PADDING_SPACING)}{headerLine}";
                               return $"""


                                       {new string(PADDING_CHAR, headerLine.Length + PADDING_SPACING + LEFT_AND_RIGHT_PADDING * 2)}
                                       {headerLine.PadLeft(headerLine.Length + LEFT_AND_RIGHT_PADDING, PADDING_CHAR)}{new string(' ', PADDING_SPACING)}{new string(PADDING_CHAR, LEFT_AND_RIGHT_PADDING)}
                                       {new string(PADDING_CHAR, headerLine.Length + PADDING_SPACING + LEFT_AND_RIGHT_PADDING * 2)}
                                       """;
                           };
                           o.IsOmittedOnCapture = true;
                           o.Redactors = provider.GetRequiredService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                       });
                   });
        }

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

        /// <inheritdoc cref="ZLoggerAtomicConsoleLoggerProvider" />
        private ILoggingBuilder AddNitroxAtomicZLoggerConsole(Action<ZLoggerConsoleOptions, IServiceProvider> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, ZLoggerAtomicConsoleLoggerProvider>((Func<IServiceProvider, ZLoggerAtomicConsoleLoggerProvider>) (serviceProvider =>
            {
                ZLoggerConsoleOptions options = new();
                configure(options, serviceProvider);
                if (options.ConfigureEnableAnsiEscapeCode && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    WindowsConsoleMode.TryEnableVirtualTerminalProcessing();
                }
                if (options.OutputEncodingToUtf8)
                {
                    Console.OutputEncoding = new UTF8Encoding(false);
                }
                return new ZLoggerAtomicConsoleLoggerProvider(options);
            }));
            return builder;
        }

        private ILoggingBuilder AddNitroxZLoggerPlain(Action<ZLoggerPlainOptions, IServiceProvider> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, ZLoggerPlainLoggerProvider>(provider =>
            {
                PlainLogProcessor processor = new() { Options = new() };
                configure(processor.Options, provider);
                processor.Formatter = processor.Options.CreateFormatter();
                return new ZLoggerPlainLoggerProvider(processor, processor.Options);
            });
            return builder;
        }
    }
}
