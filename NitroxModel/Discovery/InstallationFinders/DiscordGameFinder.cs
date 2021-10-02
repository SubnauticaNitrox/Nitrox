using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class DiscordGameFinder : IFindGameInstallation
    {
        /// <summary>
        ///     Subnautica Discord is either in appdata or in C:. So for now we just check these 2 paths until we have a better way.
        /// </summary>
        /// <returns></returns>
        public string FindGame(IList<string> errors = null)
        {
            bool HasGameInDir(string path)
            {
                return File.Exists(Path.Combine(path, GameInfo.Subnautica.ExeName));
            }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiscordGames", "Subnautica", "content");
            if (HasGameInDir(path))
            {
                return path;
            }
            path = @"C:\Games\Subnautica\content";
            if (HasGameInDir(path))
            {
                return path;
            }

            return null;
        }
    }
}