using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class FirewallModule : TroubleshootModule
    {
        public const string NAME = "Firewall";

        public FirewallModule() : base(NAME) { }

        protected override bool? Check()
        {
            throw new System.NotImplementedException();
        }
    }
}
