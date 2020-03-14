using System.Security.Principal;

namespace NitroxLauncher
{
    public class RoleDetection
    {
        public static bool isAppRunningInAdmin()
        {
            WindowsPrincipal wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
