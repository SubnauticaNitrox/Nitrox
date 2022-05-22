using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class PermissionsModule : TroubleshootModule
    {
        public PermissionsModule()
        {
            Name = "Permissions";
        }

        protected override bool Check()
        {
            throw new System.NotImplementedException();
        }
    }
}
