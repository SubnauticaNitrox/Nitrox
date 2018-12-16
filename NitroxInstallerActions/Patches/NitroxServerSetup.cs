using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace InstallerActions.Patches
{
    public static class NitroxServerSetup
    {
        private static string assemblyCSharpDLL = "Assembly-CSharp.dll";
        private static string serverAssemblyCSharpDLL = "..\\..\\SubServer\\" + assemblyCSharpDLL;

        private static string assemblyCSharpfirstpassDLL = "Assembly-CSharp-firstpass.dll";
        private static string serverAssemblyCSharpfirstpassDLL = "..\\..\\SubServer\\" + assemblyCSharpfirstpassDLL;

        // dir = (Subnautica Directory)\Subnautica_Data\Managed
        public static void Init(string dir)
        {
            string file = Path.Combine(Path.GetFullPath(dir), serverAssemblyCSharpDLL); // (Subnautica Directory)\SubServer\Assembly-CSharp.dll
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Copy(dir + assemblyCSharpDLL, file);

            file = Path.Combine(Path.GetFullPath(dir), serverAssemblyCSharpfirstpassDLL); // (Subnautica Directory)\SubServer\Assembly-CSharp-firstpass.dll
            if (File.Exists(serverAssemblyCSharpfirstpassDLL))
            {
                File.Delete(serverAssemblyCSharpfirstpassDLL);
            }
            File.Copy(dir + assemblyCSharpfirstpassDLL, file);

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Newtonsoft.Json.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Newtonsoft.Json.dll"));
            }
            File.Copy(dir + "Newtonsoft.Json.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Newtonsoft.Json.dll"));
            
            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.dll"));
            }
            File.Copy(dir + "UnityEngine.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.UI.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.UI.dll"));
            }
            File.Copy(dir + "UnityEngine.UI.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\UnityEngine.UI.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Poly2Tri.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Poly2Tri.dll"));
            }
            File.Copy(dir + "Poly2Tri.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\Poly2Tri.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LitJson.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LitJson.dll"));
            }
            File.Copy(dir + "LitJson.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LitJson.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\iTween.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\iTween.dll"));
            }
            File.Copy(dir + "iTween.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\iTween.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LibNoise.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LibNoise.dll"));
            }
            File.Copy(dir + "LibNoise.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LibNoise.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LumenWorks.dll")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LumenWorks.dll"));
            }
            File.Copy(dir + "LumenWorks.dll", Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\LumenWorks.dll"));

            if (File.Exists(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\path.txt")))
            {
                File.Delete(Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\path.txt"));
            }
            File.WriteAllText(file, Path.Combine(dir, "..\\..\\"));

        }

    }
}
