using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class GameInCurrentDirectoryFinder : IFindGameInstallation
    {
        public string FindGame(IList<string> errors = null)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(currentDirectory, "Subnautica.exe")))
            {
                return currentDirectory;
            }

            return null;
        }
    }
}
