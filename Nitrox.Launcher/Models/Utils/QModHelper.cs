using System.IO;

namespace Nitrox.Launcher.Models.Utils;

internal static class QModHelper
{
    internal static bool IsQModInstalled(string subnauticaBasePath)
    {
        string subnauticaQModManagerPath = Path.Combine(subnauticaBasePath, "Bepinex", "plugins", "QModManager");
        return Directory.Exists(subnauticaQModManagerPath);
    }
}
