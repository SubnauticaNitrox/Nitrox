namespace NitroxModel
{
    public sealed class GameInfo
    {
        public static readonly GameInfo Subnautica = new()
        {
            Name = "Subnautica",
            FullName = "Subnautica",
            ExeName = "Subnautica.exe",
            SteamAppId = 264710
        };

        public static readonly GameInfo SubnauticaBelowZero = new()
        {
            Name = "SubnauticaZero",
            FullName = "Subnautica: Below Zero",
            ExeName = "SubnauticaZero.exe",
            SteamAppId = 848450
        };

        public string Name { get; private set; }
        public string FullName { get; private set; }
        public string ExeName { get; private set; }
        public int SteamAppId { get; private set; }

        private GameInfo()
        {
        }
    }
}
