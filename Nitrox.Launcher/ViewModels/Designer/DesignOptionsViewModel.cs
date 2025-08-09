using Nitrox.Launcher.Models.Design;
using NitroxModel.Discovery.Models;

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
        LogsFolderDir = @"C:\Users\Me\AppData\Roaming\Nitrox\Logs";
    }
}
