using System.IO;

namespace InstallerActions.Patches
{
    public static class NitroxServerSetup
    {
        private static string[] files = new string[] 
        {
            "Assembly-CSharp.dll",
            "Assembly-CSharp-firstpass.dll",
            "Newtonsoft.Json.dll",
            "UnityEngine.dll",
            "UnityEngine.UI.dll",
            "Poly2Tri.dll",
            "LitJson.dll",
            "iTween.dll",
            "LibNoise.dll",
            "LumenWorks.dll",
        };

        // dir = (Subnautica Directory)\Subnautica_Data\Managed
        public static void Init(string dir)
        {
            foreach (string tmpFile in files)
            {
                CopyFile(tmpFile, dir);
            }

            string file = Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\path.txt");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            string tmpDir = dir.Replace("\\Subnautica_Data\\Managed", "");
            File.WriteAllText(file, tmpDir);
        }

        private static void CopyFile(string fileName, string dir)
        {
            string file = Path.Combine(Path.GetFullPath(dir), "..\\..\\SubServer\\" + fileName); // (Subnautica Directory)\SubServer\fileName
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.Copy(dir + fileName, file);
        }

    }
}
