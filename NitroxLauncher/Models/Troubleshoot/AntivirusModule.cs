using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class AntivirusModule : TroubleshootModule
    {
        public const string NAME = "Anti-virus";

        public AntivirusModule() : base(NAME) { }

        protected override bool? Check()
        {
            throw new System.NotImplementedException();
        }
    }
}
