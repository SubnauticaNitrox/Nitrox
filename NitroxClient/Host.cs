using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Model;
using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours;
using NitroxClient.Services;
using NitroxClient.Services.Game;

namespace NitroxClient;

internal static class Host
{
    public static void Initialize()
    {
        Task.Run(() =>
        {
            try
            {
                Setup();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"[Nitrox] [ERROR] Failed to load one or more dependency types for Nitrox. Assembly: {ex.Types.FirstOrDefault()?.Assembly.FullName ?? "unknown"}");
                foreach (Exception loaderEx in ex.LoaderExceptions)
                {
                    Console.WriteLine($"[Nitrox] [ERROR] {loaderEx}");
                }
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while initializing and loading dependencies.", ex);
            }
            return Task.CompletedTask;
        }).ContinueWithHandleError(ex => Console.WriteLine($"[Nitrox] [ERROR] {ex}"));

        // Blocks the game thread until Nitrox has finished patching / loading.
        ContinueGameStartAfterHostReadyService.WaitForHostAsync().GetAwaiter().GetResult();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Setup()
    {
        // TODO: Save player preferences (player name, color)
        // TODO: Call multiplayer start / joined callbacks
        // TODO: Multiplayer monobehaviour to services

        HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            EnvironmentName = NitroxEnvironment.DotnetEnvironment,
            ApplicationName = NitroxEnvironment.AppName
        });
        builder.Configuration
               .AddCommandLine(Environment.GetCommandLineArgs())
            ;
        // TODO: Use console logging
        builder.Logging
               .AddSimpleConsole()
            ;
        builder.Services
               .Configure<HostOptions>(options =>
               {
                   options.ServicesStartConcurrently = true;
                   options.ServicesStopConcurrently = true;
               })
               .AddSingleton(GameInfo.Subnautica)
               .AddDebugFeatures()
               .AddPackets()
               .AddPatches()
               .AddGameServices()
               .AddGameLogicHelpers()
               .AddInitialSyncProcessors()
               .AddMetadataProcessing()
               .AddScoped<PacketProcessorsInvoker>()
               .AddSingleton<IClient>(provider => provider.GetRequiredService<LiteNetLibClientService>())
               .AddSingleton<JoinServerBackend>()
               .AddHostedSingletonService<ContinueGameStartAfterHostReadyService>()
               .AddHostedSingletonService<PacketSerializationService>()
               .AddHostedSingletonService<NitroxProtobufSerializerService>()
               .AddHostedSingletonService<NitroxSettingsService>()
               .AddHostedSingletonService<StatusService>()
               .AddHostedSingletonService<PatcherService>()
               .AddHostedSingletonService<WarnIfOtherModsService>()
            ;

        IHost host = builder.Build();
        NitroxBootstrapper.Initialize(host.Services);
        _ = host.RunAsync().ContinueWithHandleError();
    }
}
