using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class LaunchGamePageViewModel : RoutableViewModelBase
{
    public LaunchGamePageViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    [RelayCommand]
    private void StartSingleplayer()
    {
        //try
        //{
        //    await LauncherLogic.Instance.StartSingleplayerAsync();
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show(ex.ToString(), "Error while starting in singleplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
    }
    
    [RelayCommand]
    private void StartMultiplayer()
    {
        //try
        //{
        //    await LauncherLogic.Instance.StartMultiplayerAsync();
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show(ex.ToString(), "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
    }
}