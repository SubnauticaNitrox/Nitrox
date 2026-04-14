using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Launcher.ViewModels.Designer;

internal class DesignOptionsViewModel : OptionsViewModel
{
    public DesignOptionsViewModel() : base(null!, null!, null!, null!)
    {
        SelectedGame = new KnownGame
        {
            PathToGame = @"C:\Games\Steam\Subnautica",
            Platform = Platform.STEAM
        };
        KnownGames =
        [
            new KnownGame
            {
                PathToGame = @"C:\Games\Steam\Subnautica",
                Platform = Platform.STEAM
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\Epic\Subnautica",
                Platform = Platform.EPIC
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\MicrosoftStore\Subnautica",
                Platform = Platform.MICROSOFT
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\HeroicGames\Subnautica",
                Platform = Platform.HEROIC
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\Discord\Subnautica",
                Platform = Platform.DISCORD
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\UhOh\Subnautica",
                Platform = Platform.NONE
            }
        ];
        LaunchArgs = "-vrmode none";
        ProgramDataPath = @"C:\Users\Me\AppData\Roaming\Nitrox";
        ScreenshotsPath = @"C:\Users\Me\AppData\Roaming\Nitrox\screenshots";
        SavesPath = @"C:\Users\Me\AppData\Roaming\Nitrox\saves";
        LogsPath = @"C:\Users\Me\AppData\Roaming\Nitrox\logs";
    }
}
