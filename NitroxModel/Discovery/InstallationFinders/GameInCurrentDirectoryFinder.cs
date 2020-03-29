using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class GameInCurrentDirectoryFinder : IFindGameInstallation
    {
        public Optional<string> FindGame(List<string> errors = null)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(currentDirectory, "Subnautica.exe")))
            {
                return Optional<string>.Of(currentDirectory);
            }

            return Optional.Empty;
        }
    }
}
