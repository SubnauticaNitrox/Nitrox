using System.ComponentModel;
using NitroxModel;

namespace NitroxLauncher.Models.Troubleshoot
{
    public class TroubleshootModule
    {
        public string StatusCode => Status.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";

        public TroubleshootStatus Status { get; protected set; }

        public string Name { get; }

        public TroubleshootModule(string name)
        {
            Name = name;
            Status = TroubleshootStatus.NOT_STARTED;
        }

        public bool? Check()
        {
            return true;
        }
    }
}
