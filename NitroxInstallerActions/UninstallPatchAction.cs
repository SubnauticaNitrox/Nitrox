using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace NitroxInstallerActions
{
    public class UninstallPatchAction
    {
        [CustomAction]
        public static ActionResult UninstallPatch(Session session)
        {
            session.Log("Begin uninstall");
            MessageBox.Show("uninstall", "uninstall");
            return ActionResult.Success;
        }
    }
}
