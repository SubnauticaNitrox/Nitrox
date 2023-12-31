using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Avalonia.Collections;
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
        // Dynamically get the image names contained within the project's "gallery-images" folder
        foreach (string imageName in Directory.GetFiles($@"{Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))}\Assets\Images\gallery-images"))
        {
            GalleryImageSources.Add(imageName.Replace(@"D:\Code\Nitrox\Nitrox.Launcher", "")); // HARDCODED VALUE BASED ON MY LOCAL PATH - Needs to be fixed
        }

        // TEMP
        foreach (string value in GalleryImageSources)
        {
            Console.WriteLine($"Adding image source \"{value}\"");
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