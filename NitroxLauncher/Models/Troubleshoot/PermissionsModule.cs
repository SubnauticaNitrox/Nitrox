using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class PermissionsModule : TroubleshootModule
    {
        public const string NAME = "Permissions";

        public PermissionsModule() : base(NAME) { }

        protected override bool? Check()
        {
            throw new System.NotImplementedException();
        }
    }
}
