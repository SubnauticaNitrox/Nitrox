using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class DiscordGameFinder : IFindGameInstallation
    {
        /// <summary>
        ///     Subnautica Discord is either in appdata or in C:. So for now we just check these 2 paths until we have a better way.
        ///     Discord stores game files in a subfolder called "content" while the parent folder is used to store Discord related files instead.
        /// </summary>
        public string FindGame(IList<string> errors = null)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiscordGames", "Subnautica", "content");
            if (HasSubnautica(path))
            {
                return path;
            }
            path = @"C:\Games\Subnautica\content";
            if (HasSubnautica(path))
            {
                return path;
            }

            return null;
        }

        private bool HasSubnautica(string path)
        {
            return File.Exists(Path.Combine(path, GameInfo.Subnautica.ExeName));
        }
    }
}