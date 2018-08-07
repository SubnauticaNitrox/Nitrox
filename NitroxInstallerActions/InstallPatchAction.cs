using Microsoft.Deployment.WindowsInstaller;

namespace NitroxInstallerActions
{
    public class InstallPatchAction
    {
        [CustomAction]
        public static ActionResult InstallPatch(Session session)
        {
            session.Log("Begin CustomAction1");

            return ActionResult.Success;
        }
    }
}
