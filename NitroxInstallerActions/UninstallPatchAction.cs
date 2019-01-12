using System;
using InstallerActions.Patches;
using Microsoft.Deployment.WindowsInstaller;

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
