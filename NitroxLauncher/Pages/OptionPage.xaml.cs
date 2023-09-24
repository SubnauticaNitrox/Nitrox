using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxLauncher.Models;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxServer.Serialization.World;

namespace NitroxLauncher.Pages;

public partial class OptionPage : PageBase
{
    public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
    public string PathToSubnautica => NitroxUser.GamePath;
    public string SubnauticaLaunchArguments => LauncherLogic.Config.LaunchArguments;

    [ObservableProperty]
    public ObservableCollection<GameInstallation> gameInstallations = new();

    [ObservableProperty]
    private GameInstallation selectedGameInstallation = default;

    partial void OnSelectedGameInstallationChanged(GameInstallation value)
    {
        _ = LauncherLogic.Instance.SetTargetedSubnauticaPath(value.Path);
    }

    public OptionPage()
    {
        InitializeComponent();
        SaveFileLocationTextblock.Text = WorldManager.SavesFolderDir;

        ArgumentsTextbox.Text = SubnauticaLaunchArguments;
        if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
        {
            ResetButton.Visibility = Visibility.Visible;
        }

        _ = Dispatcher.InvokeAsync(() =>
        {
            IEnumerable<GameInstallation> data = GameInstallationFinder.Instance.FindGame(GameInfo.Subnautica, GameLibraries.PLATFORMS, null);
            GameInstallations = new(data);

            GameInstallation currentInstallation = GameInstallations.Where(x => x.Path == NitroxUser.PreferredGamePath).FirstOrDefault();
            if (currentInstallation == default)
            {
                currentInstallation = new()
                {
                    Origin = GameLibraries.CONFIG,
                    GameInfo = GameInfo.Subnautica,
                    Path = NitroxUser.PreferredGamePath
                };
                
                GameInstallations.Add(currentInstallation);
            }

            selectedGameInstallation = currentInstallation;
        });

        Loaded += (s, e) =>
        {
            LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
            OnLogicPropertyChanged(null, null);
        };

        Unloaded += (s, e) =>
        {
            LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
        };
    }

    private void OnChangePath_Click(object sender, RoutedEventArgs e)
    {
        string selectedDirectory;

        // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
        using (CommonOpenFileDialog dialog = new()
        {
            Multiselect = false,
            InitialDirectory = PathToSubnautica,
            EnsurePathExists = true,
            IsFolderPicker = true,
            Title = "Select Subnautica installation directory"
        })
        {
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            selectedDirectory = Path.GetFullPath(dialog.FileName);
        }

        if (!GameInstallationFinder.HasGameExecutable(selectedDirectory, GameInfo.Subnautica))
        {
            LauncherNotifier.Error("Invalid subnautica directory");
            return;
        }

        if (selectedDirectory != PathToSubnautica)
        {
            GameInstallation gameInstallation = new()
            {
                Origin = GameLibraries.CONFIG,
                GameInfo = GameInfo.Subnautica,
                Path = selectedDirectory
            };

            GameInstallations.Add(gameInstallation);
            SelectedGameInstallation = gameInstallation;
            LauncherNotifier.Success("Applied changes");
        }
    }

    private void OnChangeArguments_Click(object sender, RoutedEventArgs e)
    {
        if (ArgumentsTextbox.Text == SubnauticaLaunchArguments)
        {
            return;
        }

        ResetButton.Visibility = SubnauticaLaunchArguments == LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS ? Visibility.Visible : Visibility.Hidden;
        ArgumentsTextbox.Text = LauncherLogic.Config.LaunchArguments = ArgumentsTextbox.Text.Trim();
        LauncherNotifier.Success("Applied changes");
    }

    private void OnResetArguments_Click(object sender, RoutedEventArgs e)
    {
        if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
        {
            ArgumentsTextbox.Text = LauncherLogic.Config.LaunchArguments = LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS;
            ResetButton.Visibility = Visibility.Hidden;
            LauncherNotifier.Success("Applied changes");
            return;
        }
    }

    private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        OnPropertyChanged(nameof(PathToSubnautica));
        OnPropertyChanged(nameof(GamePlatform));
    }

    private void OnViewFolder_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(WorldManager.SavesFolderDir)?.Dispose();
    }
}
