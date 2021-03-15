using System.IO;

namespace NitroxLauncher.Patching
{
    public static class QModHelper
    {
        private static readonly string originalFileName = "winhttp.dll";
        private static readonly string renamedFileName = "winhttp_nitrox_stopped.dll";

        public static void RemoveQModEntryPoint(string subnauticaBasePath)
        {
            RenameFile(subnauticaBasePath, originalFileName, renamedFileName);
        }

        public static void RestoreQModEntryPoint(string subnauticaBasePath)
        {
            RenameFile(subnauticaBasePath, renamedFileName, originalFileName);
        }

        private static void RenameFile(string subnauticaBasePath, string fileToRename, string newFileName)
        {
            string fileToRenamePath = Path.Combine(subnauticaBasePath, fileToRename);
            if (!File.Exists(fileToRenamePath))
            {
                return;
            }

            string newFilePath = Path.Combine(subnauticaBasePath, newFileName);
            File.Move(fileToRenamePath, newFilePath);
        }
    }
}
