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
                string gameDir = Path.GetDirectoryName(Path.Combine(Path.GetFullPath(managedDirectory), "..","..","Subnautica.exe"));
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
                string[] requiredRules = FirewallRules.GetRequiredRules();
                string status = FirewallRules.SetFirewallRules(requiredRules, gameDir);
                if (status == "fail")
                {
                    MessageBox.Show("You may need to add firewall exceptions manually");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
