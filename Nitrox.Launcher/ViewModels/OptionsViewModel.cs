using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class OptionsViewModel : RoutableViewModelBase
{
    //public AvaloniaList<KnownGame> KnownGames { get; init; }

    public static string SavesFolderdir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves"); // Temp
    
    private static string defaultLaunchArg => "-vrmode none";
    
    [ObservableProperty]
    private KnownGame selectedGame;

    // Temp - Meant to represent the value of LauncherLogic.Config.SubnauticaLaunchArguments
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeArgumentsCommand))]
    private string launcherLogicConfigSubnauticaLaunchArguments = defaultLaunchArg;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeArgumentsCommand))]
    private string launchArgs;
    
    [ObservableProperty]
    private bool showResetArgsBtn;
    
    public OptionsViewModel(IScreen hostScreen) : base(hostScreen)
    {
        SelectedGame = new()
        {
            PathToGame = NitroxUser.GamePath,
            Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE
        };
        //KnownGames =
        //[
        //    new()
        //    {
        //        PathToGame = NitroxUser.GamePath,
        //        Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE
        //    }
        //];

        LaunchArgs = LauncherLogicConfigSubnauticaLaunchArguments/*LauncherLogic.Config.SubnauticaLaunchArguments*/;
    }

    [RelayCommand]
    private async void ChangePath()
    {
        // TODO: Maybe use Window.StorageProvider API instead of OpenFileDialog
        OpenFolderDialog dialog = new()
        {
            Title = "Select Subnautica installation directory",
            Directory = new(SelectedGame.PathToGame)
        };
        string selectedDirectory = await dialog.ShowAsync(MainWindow) ?? "";
        
        if (selectedDirectory == "")
        {
            LaunchArgs = "Cancelled";    //TEMP
            return;
        }
        
        if (!GameInstallationHelper.HasGameExecutable(selectedDirectory, GameInfo.Subnautica))
        {
            //LauncherNotifier.Error("Invalid subnautica directory");
            LaunchArgs = "Invalid subnautica directory";    //TEMP
            return;
        }
        
        if (selectedDirectory != SelectedGame.PathToGame)
        {
            //await LauncherLogic.Instance.SetTargetedSubnauticaPath(selectedDirectory);
            //LauncherNotifier.Success("Applied changes");
            LaunchArgs = "Applied changes";    //TEMP
        }
    }

    //[RelayCommand]
    //private void AddGameInstallation()
    //{
    //}

    [RelayCommand]
    private void ResetArguments()
    {
        LaunchArgs = defaultLaunchArg;
        ShowResetArgsBtn = false;
    }
    
    [RelayCommand(CanExecute = nameof(CanChangeArguments))]
    private void ChangeArguments()
    {
        LauncherLogicConfigSubnauticaLaunchArguments/*LauncherLogic.Config.SubnauticaLaunchArguments*/ = LaunchArgs;
    }
    private bool CanChangeArguments()
    {
        if (LaunchArgs != defaultLaunchArg)
        {
            ShowResetArgsBtn = true;
        }

        return LaunchArgs != LauncherLogicConfigSubnauticaLaunchArguments/*LauncherLogic.Config.SubnauticaLaunchArguments*/;
    }

    [RelayCommand]
    private void ViewFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = SavesFolderdir,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }

    public class KnownGame
    {
        public string PathToGame { get; init; }
        public Platform Platform { get; init; }
    }
}
