using System;
using InstallerActions;
using InstallerActions.Patches;
using Microsoft.Deployment.WindowsInstaller;

namespace NitroxInstallerActions
{
    public class InstallPatchAction
    {
        [CustomAction]
        public static ActionResult InstallPatch(Session session)
        {
            System.Diagnostics.Debugger.Launch();
            session.Log("Begin install");

            try
            {
                string managedDirectory = session.CustomActionData["MANAGEDDIR"];
                NitroxEntryPatch nitroxPatch = new NitroxEntryPatch(managedDirectory);

                if (!nitroxPatch.IsApplied)
                {
                    nitroxPatch.Apply();
                }
            }
            catch(Exception ex)
            {
                session.Log(ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}
