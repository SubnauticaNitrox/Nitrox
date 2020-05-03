using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NitroxModel.Discovery;
using NitroxModel.Logger;

namespace NitroxModel.Helper
{
    public static class NitroxUtils
    {
        private static bool? isUnityApiAvailable;
        
        public static bool IsUnityApiAvailable
        {
            get
            {
                if (isUnityApiAvailable != null)
                {
                    return isUnityApiAvailable.Value;
                }
                
                try
                {
                    MethodInfo unityLogMethod = AppDomain.CurrentDomain.GetAssemblies()
                                                     .FirstOrDefault(a => a.FullName.StartsWith("UnityEngine"))
                                                     ?.GetType("UnityEngine.Debug")
                                                     ?.GetMethod("Log", new [] { typeof(string) });
                    if (unityLogMethod != null && unityLogMethod.Invoke(null, new object[] { "Unity API test" }) == null)
                    {
                        isUnityApiAvailable = true;
                        Log.Debug($"{nameof(NitroxUtils)}: Unity API available");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (!e.ToString().Contains("System.Security.SecurityException"))
                    {
                        Log.Error($"{nameof(IsUnityApiAvailable)}", e);
                    }
                }
                isUnityApiAvailable = false;
                Log.Debug($"{nameof(NitroxUtils)}: Unity API unavailable");
                return false;
            }
        }

        private static string subnauticaPath;

        public static string SubnauticaPath => subnauticaPath ?? (subnauticaPath = Path.Combine(GameInstallationFinder.Instance.FindGame().OrElse("")));


        public static string SubnauticaManagedLibsPath => Path.Combine(SubnauticaPath, "Subnautica_Data", "Managed");
    }
}
