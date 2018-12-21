using System;
using System.IO;
using System.Windows.Forms;
using InstallerActions.Patches;
using Microsoft.Deployment.WindowsInstaller;

namespace NitroxInstallerActions
{
    public class InstallPatchAction
    {
        [CustomAction]
        public static ActionResult InstallPatch(Session session)
        {
            session.Log("Begin install");

            try
            {
                string managedDirectory = session.CustomActionData["MANAGEDDIR"];
                                
                if (!RequiredAssembliesExist(managedDirectory))
                {
                    MessageBox.Show("Error installing Nitrox to the specified directory. Please ensure the installer is pointing to your subnautica directory and try again.  Attempting to locate managed at: " + managedDirectory);
                    return ActionResult.Failure;
                }

                NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(managedDirectory);

                NitroxServerSetup.Init(managedDirectory);

                if (!nitroxPatch.IsApplied)
                {
                    nitroxPatch.Apply();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                session.Log(ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        private static bool RequiredAssembliesExist(string managedDirectory)
        {
            return File.Exists(managedDirectory + NitroxEntryPatch.GAME_ASSEMBLY_NAME);
        }
    }
}
