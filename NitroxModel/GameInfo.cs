using System.IO;
using System.Runtime.InteropServices;

namespace NitroxModel;

public sealed class GameInfo
{
    public static readonly GameInfo Subnautica;

    public static readonly GameInfo SubnauticaBelowZero;

    public string Name { get; private set; }

    public string FullName { get; private set; }

    public string DataFolder { get; private set; }

    public string ExeName { get; private set; }

    public int SteamAppId { get; private set; }

    public string MsStoreStartUrl { get; private set; }

    static GameInfo()
    {
        Subnautica = new GameInfo
        {
            Name = "Subnautica",
            FullName = "Subnautica",
            DataFolder = "Subnautica_Data",
            ExeName = "Subnautica.exe",
            SteamAppId = 264710,
            MsStoreStartUrl = @"ms-xbl-38616e6e:\\"
        };

        SubnauticaBelowZero = new GameInfo
        {
            Name = "SubnauticaZero",
            FullName = "Subnautica: Below Zero",
            DataFolder = "SubnauticaZero_Data",
            ExeName = "SubnauticaZero.exe",
            SteamAppId = 848450,
            MsStoreStartUrl = @"ms-xbl-6e27970f:\\"
        };

        // Fixup for OSX
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Subnautica.ExeName = "Subnautica";
            Subnautica.DataFolder = Path.Combine("Resources", "Data");
        }
    }

    private GameInfo()
    {
    }
}
