using System.IO;
using System.Runtime.InteropServices;

namespace NitroxModel;

public sealed record GameInfo
{
    public static readonly GameInfo Subnautica;

    public static readonly GameInfo SubnauticaBelowZero;

    public required string Name { get; init; }

    public required string FullName { get; init; }

    public required string DataFolder { get; init; }

    public required string ExeName { get; init; }

    public required int SteamAppId { get; init; }

    public required string MsStoreStartUrl { get; init; }

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
            Subnautica = Subnautica with
            {
                ExeName = "Subnautica",
                DataFolder = Path.Combine("Resources", "Data")
            };
        }
    }

    private GameInfo()
    {
    }
}
