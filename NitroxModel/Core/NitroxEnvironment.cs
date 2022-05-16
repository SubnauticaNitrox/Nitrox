using System;
using System.IO;
using System.Reflection;

namespace NitroxModel.Helper
{
    /// <summary>
    ///     Environment helper for getting meta data about where and how Nitrox is running.
    /// </summary>
    public static class NitroxEnvironment
    {
        public static string ReleasePhase => IsReleaseMode ? "Alpha" : "InDev";
        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public static DateTime BuildDate => File.GetCreationTimeUtc(Assembly.GetExecutingAssembly().Location);

        public static Types Type { get; private set; } = Types.NORMAL;
        public static bool IsTesting => Type == Types.TESTING;
        public static bool IsNormal => Type == Types.NORMAL;

        public static bool IsReleaseMode
        {
            get
            {
#if RELEASE
                return true;
#else
                return false;
#endif
            }
        }

        private static bool hasSet;
        public static void Set(Types value)
        {
            if (hasSet)
            {
                throw new Exception("Environment type can only be set once");
            }

            Type = value;
            hasSet = true;
        }

        public enum Types
        {
            NORMAL,
            TESTING
        }
    }
}
