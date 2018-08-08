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
            System.Diagnostics.Debugger.Launch();
            session.Log("Begin uninstall");

            try
            {
                string managedDirectory = session.CustomActionData["MANAGEDDIR"];
                NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(managedDirectory);

                if (nitroxPatch.IsApplied)
                {
                    nitroxPatch.Remove();
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
