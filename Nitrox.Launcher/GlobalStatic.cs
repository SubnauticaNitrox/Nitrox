using Avalonia.Controls;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal static class GlobalStatic
{
    public static bool IsDesignMode => Design.IsDesignMode;

    /// <inheritdoc cref="ProcessEx.OpenUrl" />
    public static void OpenUrl(string url) => ProcessEx.OpenUrl(url);
}
