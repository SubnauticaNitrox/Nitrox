using System.Management;
using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class AntivirusModule : TroubleshootModule
    {
        public AntivirusModule()
        {
            Name = "Anti-virus";
        }

        /// class AntivirusProduct {
        ///     string displayName;                // Application name
        ///     string instanceGuid;               // Unique identifier
        ///     string pathToSignedProductExe;     // Path to application
        ///     string pathToSignedReportingExe;   // Path to provider
        ///     uint32 productState;               // Real-time protection & definition state
        /// }
        protected override bool Check()
        {
            // SecurityCenter2 has been used since Windows XP
            ManagementObjectSearcher wmiData = new(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
            ManagementObjectCollection data = wmiData.Get();

            EmitLog($"Found {data.Count} products");
            // https://social.msdn.microsoft.com/Forums/fr-FR/6501b87e-dda4-4838-93c3-244daa355d7c/wmisecuritycenter2-productstate?forum=vblanguage
            foreach (ManagementObject item in data)
            {
                string name = item["displayName"]?.ToString() ?? "Unknown";

                EmitLog($"{name}\n - GUID: {item["instanceGuid"]}\n - productState : {item["productState"]}");
            }

            return true;
        }
    }
}
