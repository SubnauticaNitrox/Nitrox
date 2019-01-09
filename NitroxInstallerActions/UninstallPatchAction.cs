using System;
using InstallerActions.Patches;
using Microsoft.Deployment.WindowsInstaller;
using System.Windows.Forms;

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
                NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(managedDirectory);

                if (nitroxPatch.IsApplied)
                {
                    nitroxPatch.Remove();
                }

                NitroxServerSetup.Uninstall(managedDirectory);
                string[] installedRules = FirewallRules.GetInstalledRules();
                string status = FirewallRules.RemoveFirewallRules(installedRules);
                if (status == "fail")
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
