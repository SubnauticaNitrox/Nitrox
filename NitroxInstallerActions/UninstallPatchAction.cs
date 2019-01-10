using System;
using InstallerActions.Patches;
using Microsoft.Deployment.WindowsInstaller;
using System.Windows.Forms;
using System.IO;

namespace NitroxInstallerActions
{
    public class UninstallPatchAction
    {
        [CustomAction]
        public static ActionResult UninstallPatch(Session session)
        {
            session.Log("Begin uninstall");

            try
            {
                string managedDirectory = session.CustomActionData["MANAGEDDIR"];
                string gameDir = Path.GetDirectoryName(Path.Combine(Path.GetFullPath(managedDirectory), "..", "..", "Subnautica.exe"));
                NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(managedDirectory);

                if (nitroxPatch.IsApplied)
                {
                    nitroxPatch.Remove();
                }

                NitroxServerSetup.Uninstall(managedDirectory);
                FirewallRules removeRules = new FirewallRules(gameDir);
                bool status=removeRules.RemoveInstalledRules();
                if (!status)
                {
                    MessageBox.Show("You may need to remove some firewall exceptions manually");
                }
            }
            catch (Exception ex)
            {
                session.Log(ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}
