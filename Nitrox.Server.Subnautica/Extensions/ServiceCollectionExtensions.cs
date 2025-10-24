using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors.Core;
using Nitrox.Server.Subnautica.Services;
using ServiceScan.SourceGenerator;
using ZLogger.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class ServiceCollectionExtensions
{
    private static readonly Lazy<string> newWorldSeed = new(() => StringHelper.GenerateRandomString(10));

    /// <summary>
    ///     Adds the fallback implementation for the interface if no other implementation is set.
    /// </summary>
    public static IServiceCollection AddFallback<TInterface, TFallback>(this IServiceCollection services) where TInterface : class where TFallback : class, TInterface
    {
        services.TryAddSingleton<TInterface, TFallback>();
        return services;
    }

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services) where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddSingletonLazyArrayProvider<T>(this IServiceCollection services) => services.AddSingleton<Func<T[]>>(provider => () => provider.GetRequiredService<IEnumerable<T>>().ToArray());

    public static IServiceCollection AddNitroxOptions(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<ServerStartOptions, ServerStartOptions.Validator>()
                .BindConfiguration("")
                .Configure(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.GamePath))
                    {
                        options.GamePath = NitroxUser.GamePath;
                    }
                    if (string.IsNullOrWhiteSpace(options.NitroxAssetsPath))
                    {
                        options.NitroxAssetsPath = NitroxUser.AssetsPath;
                    }
                    if (string.IsNullOrWhiteSpace(options.NitroxAppDataPath))
                    {
                        options.NitroxAppDataPath = NitroxUser.AppDataPath;
                    }
                });
        services.AddOptionsWithValidateOnStart<SubnauticaServerOptions, SubnauticaServerOptions.Validator>()
                .BindConfiguration(SubnauticaServerOptions.CONFIG_SECTION_PATH)
                .Configure((SubnauticaServerOptions options, IHostEnvironment environment) =>
                {
                    options.Seed = options.Seed switch
                    {
                        null or "" when environment.IsDevelopment() => "TCCBIBZXAB",
                        null or "" => newWorldSeed.Value,
                        _ => options.Seed
                    };
                });
        return services;
    }

    public static ILoggingBuilder AddNitroxLogging(this ILoggingBuilder builder)
    {
        builder.Services.AddRedactors();
        return builder.AddZLoggerConsole(static (options, provider) => options.UseNitroxFormatter(formatterOptions =>
                      {
                          bool isEmbedded = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded;
                          formatterOptions.ColorBehavior = isEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                      }))
                      .AddZLoggerRollingFile(static (options, provider) =>
                      {
                          ServerStartOptions serverStartOptions = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value;
                          options.FilePathSelector = (timestamp, sequenceNumber) => $"{Path.Combine(serverStartOptions.GetServerLogsPath(), timestamp.ToLocalTime().ToString("yyyy-MM-dd"))}_server_{sequenceNumber:000}.log";
                          options.RollingInterval = RollingInterval.Day;
                          options.UseNitroxFormatter(formatterOptions =>
                          {
                              formatterOptions.ColorBehavior = LoggerColorBehavior.Disabled;
                              formatterOptions.UseRedaction = true;
                              formatterOptions.Redactors = provider.GetService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                          });
                      });
    }

    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddHostedSingletonService<CommandService>()
            .AddHostedSingletonService<ConsoleInputService>()
            .AddSingleton<ConsoleCommandProcessor>()
            .AddCommandHandlers();
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(IRedactor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddRedactors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(Command), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);
}
