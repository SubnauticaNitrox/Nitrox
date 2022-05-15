using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class NetworkModule : TroubleshootModule
    {
        public const string NAME = "Network";

        public NetworkModule() : base(NAME) { }

        protected override bool? Check()
        {
            throw new System.NotImplementedException();
        }
    }
}
