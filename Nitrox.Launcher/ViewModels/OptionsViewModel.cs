using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class OptionsViewModel : RoutableViewModelBase
{
    public AvaloniaList<KnownGame> KnownGames { get; init; }
    public string LaunchArgs { get; private set; } = "-vrmode none"; //LauncherLogic.Config.SubnauticaLaunchArguments

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
    }

    [RelayCommand]
    private void ChangePath()
    {
    }

    [RelayCommand]
    private void ResetArguments()
    {
    }

    [RelayCommand]
    private void ChangeArguments()
    {
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
