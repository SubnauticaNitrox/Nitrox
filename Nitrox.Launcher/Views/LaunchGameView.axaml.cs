using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace Nitrox.Launcher.Views;

public partial class LaunchGameView : RoutableViewBase<LaunchGamePageViewModel>
{
    public string PlatformToolTip => GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
    public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
    //public string Version => $"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";
    
    public LaunchGameView()
    {
        InitializeComponent();
    }

    private void LogicPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        //OnPropertyChanged(nameof(GamePlatform));
        //OnPropertyChanged(nameof(PlatformToolTip));
    }
}