namespace Nitrox.Server.Subnautica.Extensions;

internal static class SubnauticaServerOptionsExtensions
{
    public static bool ShouldAutoSave(this SubnauticaServerOptions options) => options.AutoSave && options.SaveInterval > 1000;
}
