using System.IO;
using dnlib.DotNet;

namespace NitroxLauncher.Patching
{
    public class PlatformDetection
    {
        public static bool IsEpic(string subnauticaPath)
        {
            string assemblyCSharp = Path.Combine(subnauticaPath, "Subnautica_Data", "Managed", "Assembly-CSharp.dll");

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    // TODO: Not going to work with below zero, find an alternative
                    if(type.FullName == "PlatformServicesEpic")
                    {
                        return true;
                    }
                }                
            }

            return false;
        }
    }
}
