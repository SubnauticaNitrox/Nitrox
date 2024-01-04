using System.ComponentModel.DataAnnotations;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class OptionsViewModel : RoutableViewModelBase
{
    public AvaloniaList<KnownGame> KnownGames { get; init; }

    private static string defaultLaunchArg => "-vrmode none";

    // Temp - Meant to represent the value of LauncherLogic.Config.SubnauticaLaunchArguments
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeArgumentsCommand))]
    private string launcherLogicConfigSubnauticaLaunchArguments = defaultLaunchArg;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeArgumentsCommand))]
    [Required]
    private string launchArgs;
    
    [ObservableProperty]
    private bool showResetArgsBtn; // TODO: Make reset button appear whenever the text in the textblock is different from the default SubnauticaLaunchArguments (not only when it's applied)
    
    public OptionsViewModel(IScreen hostScreen) : base(hostScreen)
    {
        KnownGames =
        [
            new()
            {
                PathToGame = NitroxUser.GamePath,
                Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE
            }
        ];

        LaunchArgs = LauncherLogicConfigSubnauticaLaunchArguments/*LauncherLogic.Config.SubnauticaLaunchArguments*/;
    }

    [RelayCommand]
    private void ChangePath()
    {
    }

    [RelayCommand]
    private void AddGameInstallation()
    {
    }

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
    }

    public class KnownGame
    {
        public string PathToGame { get; init; }
        public Platform Platform { get; init; }
    }
}
