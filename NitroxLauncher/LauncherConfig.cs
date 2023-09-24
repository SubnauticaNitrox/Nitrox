using CommunityToolkit.Mvvm.ComponentModel;
using NitroxLauncher.Properties;
using NitroxModel.Helper;

namespace NitroxLauncher;

internal sealed partial class LauncherConfig : ObservableObject
{
    private static LauncherConfig instance;
    public static LauncherConfig Instance => instance ??= new LauncherConfig(); 

    public const string DEFAULT_LAUNCH_ARGUMENTS = "-vrmode none";

    [ObservableProperty]
    private bool isUpToDate = true;

    [ObservableProperty]
    private bool isExternalServer = Settings.Default.IsExternalServer;

    [ObservableProperty]
    private string launchArguments = Settings.Default.LaunchArgs ?? "-vrmode none";

    [ObservableProperty]
    private string gamePath = NitroxUser.PreferredGamePath;

    partial void OnGamePathChanging(string oldValue, string newValue)
    {
        if (oldValue == newValue) return;

        NitroxUser.GamePath = newValue;
        NitroxUser.PreferredGamePath = newValue;
    }

    partial void OnLaunchArgumentsChanging(string oldValue, string newValue)
    {
        if (oldValue == newValue) return;

        Settings.Default.LaunchArgs = newValue;
        Settings.Default.Save();
    }

    partial void OnIsExternalServerChanging(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;

        Settings.Default.IsExternalServer = newValue;
        Settings.Default.Save();
    }
}
