using System;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Collections;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class LaunchGameViewModel : RoutableViewModelBase
{
    public string PlatformToolTip => GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
    public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
    public string Version => $"INDEV 1.8.0.0"; //$"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";
    
    [ObservableProperty]
    private AvaloniaList<string> galleryImageSources = new();
    
    public LaunchGameViewModel(IScreen hostScreen) : base(hostScreen)
    {
        foreach (Uri asset in AssetLoader.GetAssets(new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/Images/gallery-images"), null))
        {
            GalleryImageSources.Add(asset.LocalPath);
        }
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
