using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Launcher.ViewModels.Designer;

internal class DesignOptionsViewModel : OptionsViewModel
{
    public DesignOptionsViewModel() : base(null!, null!)
    {
        SelectedGame = new KnownGame
        {
            PathToGame = @"C:\Users\Me\Games\Subnautica",
            Platform = Platform.STEAM
        };
        LaunchArgs = "-vrmode none";
        ProgramDataPath = @"C:\Users\Me\AppData\Roaming\Nitrox";
        ScreenshotsPath = @"C:\Users\Me\AppData\Roaming\Nitrox\screenshots";
        SavesPath = @"C:\Users\Me\AppData\Roaming\Nitrox\saves";
        LogsPath = @"C:\Users\Me\AppData\Roaming\Nitrox\logs";
    }
}
