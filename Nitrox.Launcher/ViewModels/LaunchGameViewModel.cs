using System;
using System.ComponentModel;
using System.Diagnostics; // Only used on Release
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
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
    private readonly OptionsViewModel optionsViewModel;
    public static Task<string> LastFindSubnauticaTask;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private Platform gamePlatform;
    [ObservableProperty]
    private string platformToolTip;
    [ObservableProperty]
    private AvaloniaList<string> galleryImageSources = [];

    private ProcessEx gameProcess;
    public string Version => $"{NitroxEnvironment.ReleasePhase} {NitroxEnvironment.Version}";
    public string SubnauticaLaunchArguments => KeyValueStore.Instance.GetValue("SubnauticaLaunchArguments", "-vrmode none");

    public LaunchGameViewModel(IScreen screen, IDialogService dialogService, OptionsViewModel optionsViewModel) : base(screen)
    {
        this.dialogService = dialogService;
        this.optionsViewModel = optionsViewModel;
        
        NitroxUser.GamePlatformChanged += UpdateGamePlatform;

        GamePlatform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
        PlatformToolTip = GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
        
        foreach (Uri asset in AssetLoader.GetAssets(new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/Images/gallery-images"), null))
        {
            GalleryImageSources.Add(asset.LocalPath);
        }
    }

    [RelayCommand]
    private async Task StartSingleplayerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
            {
                HostScreen.Show(optionsViewModel);
                LauncherNotifier.Warning("Location of Subnautica is unknown. Set the path to it in settings.");
                return;
            }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                LauncherNotifier.Warning("An instance of Subnautica is already running");
                return;
            }
#endif
            NitroxEntryPatch.Remove(NitroxUser.GamePath);
            gameProcess = await StartSubnauticaAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while starting game in singleplayer mode:");
            await dialogService.ShowAsync<DialogBoxViewModal>(model =>
            {
                model.Title = "Error while starting game in singleplayer mode";
                model.Description = $"Error while starting game in singleplayer mode: {ex}";
                model.DescriptionForeground = new SolidColorBrush(Colors.Red);
                model.ButtonOptions = ButtonOptions.OkClipboard;
            });
        }
    }

    [RelayCommand]
    private async Task StartMultiplayerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
            {
                HostScreen.Show(optionsViewModel);
                LauncherNotifier.Warning("Location of Subnautica is unknown. Set the path to it in settings.");
                return;
            }

            if (PirateDetection.HasTriggered)
            {
                LauncherNotifier.Error("Aarrr! Nitrox has walked the plank :(");
                return;
            }

#if RELEASE
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                LauncherNotifier.Warning("An instance of Subnautica is already running");
                return;
            }
#endif

            // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory
            string initDllName = "NitroxPatcher.dll";
            try
            {
                File.Copy(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "lib", "net472", initDllName),
                    Path.Combine(NitroxUser.GamePath, "Subnautica_Data", "Managed", initDllName),
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
            NitroxEntryPatch.Remove(NitroxUser.GamePath);
            NitroxEntryPatch.Apply(NitroxUser.GamePath);

            if (QModHelper.IsQModInstalled(NitroxUser.GamePath))
            {
                Log.Warn("Seems like QModManager is Installed");
                LauncherNotifier.Info("QModManager Detected in the game folder");
            }

            gameProcess = await StartSubnauticaAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while starting game in multiplayer mode:");
            await dialogService.ShowAsync<DialogBoxViewModal>(model =>
            {
                model.Title = "Error while starting game in multiplayer mode";
                model.Description = $"Error while starting game in multiplayer mode: {ex}";
                model.DescriptionForeground = new SolidColorBrush(Colors.Red);
                model.ButtonOptions = ButtonOptions.OkClipboard;
            });
        }
    }

    private async Task<ProcessEx> StartSubnauticaAsync()
    {
        string subnauticaPath = NitroxUser.GamePath;
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
    
    private void UpdateGamePlatform()
    {
        GamePlatform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
        PlatformToolTip = GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
    }
}
