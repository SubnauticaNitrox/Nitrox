using System.IO;
using InstallerActions.Patches;

namespace InstallerActions
{
    internal class NitroxInstallPatcher
    {
        public const string SUBNAUTICA_MANAGED_FOLDER_RELATIVE_PATH = @"\Subnautica_Data\Managed\";

        public readonly string BaseInstallDirectory;

        public string SubnauticaAssembliesPath => BaseInstallDirectory + SUBNAUTICA_MANAGED_FOLDER_RELATIVE_PATH;

        public NitroxInstallPatcher(string baseInstallDirectory)
        {
            BaseInstallDirectory = baseInstallDirectory;
        }

        internal bool RequiredAssembliesExist()
        {
            string GameInputAssemblyPath = SubnauticaAssembliesPath + NitroxEntryPatch.GAME_ASSEMBLY_NAME;

            return File.Exists(GameInputAssemblyPath);
        }

        public void RemovePatches()
        {
            NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(SubnauticaAssembliesPath);

            if (nitroxPatch.IsApplied)
            {
                nitroxPatch.Remove();
            }
        }

        public void InstallPatches()
        {
            NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(SubnauticaAssembliesPath);

            if (!nitroxPatch.IsApplied)
            {
                nitroxPatch.Apply();
            }
        }
    }
}
