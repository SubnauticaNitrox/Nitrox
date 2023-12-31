using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class OptionsViewModel : RoutableViewModelBase
{
    public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
    public string PathToSubnautica => NitroxUser.GamePath;
    public string SubnauticaLaunchArguments => "-vrmode none";//LauncherLogic.Config.SubnauticaLaunchArguments;
    
    public OptionsViewModel(IScreen hostScreen) : base(hostScreen)
    {
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
}