using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NitroxLauncher.Models;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace NitroxLauncher.Pages
{
    public partial class LaunchGamePage : PageBase
    {
        public string PlatformToolTip => GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
        public Platform GamePlatform => NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
        public string Version => $"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";

        public LaunchGamePage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged += LogicPropertyChanged;
                LogicPropertyChanged(null, null);
            };

            Unloaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged -= LogicPropertyChanged;
            };
        }

        private async void SinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            LauncherNotifier.Info("Launching singleplayer Subnautica ...");

            try
            {
                await LauncherLogic.Instance.StartSingleplayerAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting in singleplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            switch (GamePlatform)
            {
                case Platform.MICROSOFT:
                    button.Background = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Sorry, due to technical problems Nitrox isn't yet compatible with Subnautica on Microsoft Store\n\nWe're doing our best to give you the opportunity to be able to play again soon.", "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                case Platform.STEAM:

                    if (!NitroxUser.IsNewestSubnautica.GetValueOrDefault(false))
                    {
                        break;
                    }

                    button.Background = new SolidColorBrush(Colors.Red);
                    MessageBox.Show(
                        "Due to Subnautica's recent update \"Living Large\", Nitrox is currently not compatible.\nHowever you can still use older version of Subnautica in order to play Nitrox. You can do so by following these steps.\n\nENSURE NITROX AND SUBNAUTICA ARE CLOSED BEFORE PROCEEDING!\n\nChanging Subnautica to Legacy Build on STEAM:\n\n1. Right click Subnautica in Steam\n2. Click Properties\n3. Click Betas\n4. Select Legacy - Public legacy builds\n5. Close it out and let Steam download and install the Legacy version of Subnautica\n6. Run Subnautica through Steam (It will say \"Subnautica [legacy]\" in your library if you did it right)\n7. Launch Subnautica through Nitrox to play.",
                        "Nitrox requires your attention",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;

                case Platform.EPIC:

                    if (!NitroxUser.IsNewestSubnautica.GetValueOrDefault(false))
                    {
                        break;
                    }

                    button.Background = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Due to Subnautica's recent update \"Living Large\", Epic Games is currently not compatible with Nitrox.\n\nWe are working on an update we do not have an estimate as to when it is done.", "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            LauncherNotifier.Info("Launching Subnautica ...");

            try
            {
                await LauncherLogic.Instance.StartMultiplayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(GamePlatform));
            OnPropertyChanged(nameof(PlatformToolTip));
        }
    }
}
