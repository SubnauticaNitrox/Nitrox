using System.IO;
using dnlib.DotNet;

namespace NitroxLauncher.Patching
{
    public class PlatformDetection
    {
        public static bool IsSteam(string subnauticaPath)
        {
            string assemblyCSharp = Path.Combine(subnauticaPath, "Subnautica_Data", "Managed", "Assembly-CSharp.dll");

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    if(type.FullName == "PlatformServicesSteam")
                    {
                        return true;
                    }
                }                
            }

            return false;
        }
    }
}
