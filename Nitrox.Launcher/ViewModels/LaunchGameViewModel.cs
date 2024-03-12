using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Patching;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class LaunchGameViewModel : RoutableViewModelBase
{
    public static Task<string> LastFindSubnauticaTask;

    [ObservableProperty]
    private AvaloniaList<string> galleryImageSources = [];

    private ProcessEx gameProcess;
    public string PlatformToolTip => GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
    public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
    public string Version => $"{NitroxEnvironment.ReleasePhase} {NitroxEnvironment.Version}";
    public string SubnauticaPath => NitroxUser.GamePath;
    public string SubnauticaLaunchArguments => KeyValueStore.Instance.GetValue("SubnauticaLaunchArguments", "-vrmode none");

    public LaunchGameViewModel(IScreen hostScreen) : base(hostScreen)
    {
        foreach (Uri asset in AssetLoader.GetAssets(new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/Images/gallery-images"), null))
        {
            GalleryImageSources.Add(asset.LocalPath);
        }
    }

    [RelayCommand]
    private async void StartSingleplayer()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SubnauticaPath) || !Directory.Exists(SubnauticaPath))
            {
                Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<OptionsViewModel>());
                throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
            }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                throw new Exception("An instance of Subnautica is already running");
            }
#endif
            NitroxEntryPatch.Remove(SubnauticaPath);
            gameProcess = await StartSubnauticaAsync();
        }
        catch (Exception ex)
        {
            //MessageBox.Show(ex.ToString(), "Error while starting in singleplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine("Error while starting in singleplayer mode: " + ex);
        }
    }

    [RelayCommand]
    private async void StartMultiplayer()
    {
        if (string.IsNullOrWhiteSpace(SubnauticaPath) || !Directory.Exists(SubnauticaPath))
        {
            Router.Navigate.Execute(AppViewLocator.GetSharedViewModel<OptionsViewModel>());
            throw new Exception("Location of Subnautica is unknown. Set the path to it in settings.");
        }

        if (PirateDetection.HasTriggered)
        {
            throw new Exception("Aarrr! Nitrox has walked the plank :(");
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
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", initDllName),
                Path.Combine(SubnauticaPath, "Subnautica_Data", "Managed", initDllName),
                true
            );
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Unable to move initialization dll to Managed folder. Still attempting to launch because it might exist from previous runs.");
        }

        // Try inject Nitrox into Subnautica code.
        if (LastFindSubnauticaTask != null)
        {
            await LastFindSubnauticaTask;
        }
        NitroxEntryPatch.Remove(SubnauticaPath);
        NitroxEntryPatch.Apply(SubnauticaPath);

        if (QModHelper.IsQModInstalled(SubnauticaPath))
        {
            Log.Warn("Seems like QModManager is Installed");
            //LauncherNotifier.Info("Detected QModManager in the game folder");
        }

        gameProcess = await StartSubnauticaAsync();
    }

    private async Task<ProcessEx> StartSubnauticaAsync()
    {
        string subnauticaPath = SubnauticaPath;
        string subnauticaLaunchArguments = SubnauticaLaunchArguments;
        string subnauticaExe = Path.Combine(subnauticaPath, GameInfo.Subnautica.ExeName);
        IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(subnauticaPath);

        // Start game & gaming platform if needed.
        using ProcessEx game = platform switch
        {
            Steam s => await s.StartGameAsync(subnauticaExe, GameInfo.Subnautica.SteamAppId, subnauticaLaunchArguments),
            EpicGames e => await e.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            MSStore m => await m.StartGameAsync(subnauticaExe),
            //DiscordStore d => await d.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            _ => throw new Exception($"Directory '{subnauticaPath}' is not a valid {GameInfo.Subnautica.Name} game installation or the game's platform is unsupported by Nitrox.")
        };

        return game ?? throw new Exception($"Game failed to start through {platform.Name}");
    }
}
