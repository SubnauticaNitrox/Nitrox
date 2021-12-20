using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NitroxLauncher.Install;
using NitroxLauncher.Install.Core;
using NitroxLauncher.Models.Patching;
using NitroxLauncher.Models.Utils;
using NitroxLauncher.Pages;
using NitroxModel;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;
using ToastNotifications.Core;
using DiscordStore = NitroxModel.Platforms.Store.Discord;

namespace NitroxLauncher;

internal class LauncherLogic : IDisposable
{
    public const string RELEASE_PHASE = "ALPHA";

    private readonly Installer installer;
    private ProcessEx gameProcess;

    private Task<string> lastFindSubnauticaTask;
    private NitroxEntryPatch nitroxEntryPatch;
    public static string Version => Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString();

    public static LauncherLogic Instance { get; private set; }
    public static LauncherConfig Config { get; private set; }
    public static ServerLogic Server { get; private set; }
    private readonly Dispatcher mainDispatcher = Dispatcher.CurrentDispatcher;

    public LauncherLogic()
    {
        Instance = this;
        Config = new LauncherConfig();
        Server = new ServerLogic();
        installer = new Installer(() => Config.SubnauticaPath);
    }

    public void Dispose()
    {
        Application.Current.MainWindow?.Hide();

        try
        {
            nitroxEntryPatch.Remove();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while disposing the launcher");
        }

        gameProcess?.Dispose();
        Server?.Dispose();
        LauncherNotifier.Shutdown();
    }

    public IEnumerable<InstallResult> Install()
    {
        return installer.Install();
    }

    [Conditional("RELEASE")]
    public async void CheckNitroxVersion()
    {
        await Task.Factory.StartNew(async () =>
        {
            Version latestVersion = await Downloader.GetNitroxLatestVersion();
            Version currentVersion = new(Version);

            if (latestVersion > currentVersion)
            {
                Config.IsUpToDate = false;
                Log.Info($"A new version of the mod ({latestVersion}) is available");

                LauncherNotifier.Warning($"A new version of the mod ({latestVersion}) is available", new MessageOptions
                {
                    NotificationClickAction = n =>
                    {
                        NavigateTo<UpdatePage>();
                    }
                });
            }
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public async Task<string> SetTargetedSubnauticaPath(string path)
    {
        if (Config.SubnauticaPath == path || !Directory.Exists(path))
        {
            return null;
        }

        Config.SubnauticaPath = path;

        if (lastFindSubnauticaTask != null)
        {
            await lastFindSubnauticaTask;
        }

        lastFindSubnauticaTask = Task.Factory.StartNew(() =>
        {
            PirateDetection.TriggerOnDirectory(path);

            IEnumerable<InstallResult> installResults = installer.Install(i => i is InstallFilePermissions);
            string message = InstallResult.GetPrettyErrorMessage(installResults, "Nitrox might have problems running properly because some install steps failed:");
            if (message != null)
            {
                mainDispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(Application.Current.MainWindow!,
                                    message,
                                    "Potential file permission issues",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                });
            }
            
            // Save game path as preferred for future sessions.
            NitroxUser.PreferredGamePath = path;

            if (nitroxEntryPatch?.IsApplied == true)
            {
                nitroxEntryPatch.Remove();
            }
            nitroxEntryPatch = new NitroxEntryPatch(() => Config.SubnauticaPath);

            if (Path.GetFullPath(path).StartsWith(WindowsHelper.ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
            {
                WindowsHelper.RestartAsAdmin();
            }

            return path;
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

        return await lastFindSubnauticaTask;
    }

    public void NavigateTo(Type page)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }
        if (!page.IsSubclassOf(typeof(Page)) && page != typeof(Page))
        {
            throw new ArgumentException("Argument must be a type of page to navigate to", nameof(page));
        }
        if (Application.Current.MainWindow is not MainWindow window)
        {
            throw new InvalidOperationException($"{nameof(MainWindow)} is not initialized");
        }

        // Rewrite type if server is already running so it opens the correct page.
        if (Server.IsManagedByLauncher && page == typeof(ServerPage))
        {
            page = typeof(ServerConsolePage);
        }

        window.FrameContent = Application.Current.FindResource(page.Name);
    }

    public void NavigateTo<TPage>() where TPage : Page
    {
        NavigateTo(typeof(TPage));
    }

    public bool NavigationIsOn<TPage>() where TPage : Page
    {
        return Application.Current.MainWindow is MainWindow { FrameContent: TPage };
    }

    internal async Task StartSingleplayerAsync()
    {
        if (string.IsNullOrWhiteSpace(Config.SubnauticaPath) || !Directory.Exists(Config.SubnauticaPath))
        {
            NavigateTo<OptionPage>();
            throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
        }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
        nitroxEntryPatch.Remove();
        gameProcess = await StartSubnauticaAsync();
    }

    internal async Task StartMultiplayerAsync()
    {
        if (string.IsNullOrWhiteSpace(Config.SubnauticaPath) || !Directory.Exists(Config.SubnauticaPath))
        {
            NavigateTo<OptionPage>();
            throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
        }

        if (Config.IsPirated)
        {
            throw new Exception("Aarrr ! Nitrox walked the plank :(");
        }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif

        // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory 
        string initDllName = "NitroxPatcher.dll";
        try
        {
            File.Copy(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "lib", initDllName),
                Path.Combine(Config.SubnauticaPath, "Subnautica_Data", "Managed", initDllName),
                true
            );
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Unable to move initialization dll to Managed folder. Still attempting to launch because it might exist from previous runs.");
        }

        // Try inject Nitrox into Subnautica code.
        if (lastFindSubnauticaTask != null)
        {
            await lastFindSubnauticaTask;
        }
        if (nitroxEntryPatch == null)
        {
            throw new Exception("Nitrox was blocked by another program");
        }
        nitroxEntryPatch.Remove();
        nitroxEntryPatch.Apply();

        if (QModHelper.IsQModInstalled(Config.SubnauticaPath))
        {
            Log.Warn("Seems like QModManager is Installed");
            LauncherNotifier.Info("Detected QModManager in the game folder");
        }

        gameProcess = await StartSubnauticaAsync();
    }

    private async Task<ProcessEx> StartSubnauticaAsync()
    {
        string subnauticaPath = Config.SubnauticaPath;
        string subnauticaLaunchArguments = Config.SubnauticaLaunchArguments;
        string subnauticaExe = Path.Combine(subnauticaPath, GameInfo.Subnautica.ExeName);
        IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(subnauticaPath);

        // Start game & gaming platform if needed.
        using ProcessEx game = platform switch
        {
            Steam s => await s.StartGameAsync(subnauticaExe, GameInfo.Subnautica.SteamAppId, subnauticaLaunchArguments),
            EpicGames e => await e.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            MSStore m => await m.StartGameAsync(subnauticaExe),
            DiscordStore d => await d.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            _ => throw new Exception($"Directory '{subnauticaPath}' is not a valid {GameInfo.Subnautica.Name} game installation or the game's platform is unsupported by Nitrox.")
        };

        return game ?? throw new Exception($"Unable to start game through {platform.Name}");
    }
}
