using System;
using System.Reflection;

namespace NitroxModel.Helper
{
    /// <summary>
    ///     Environment helper for getting meta data about where and how Nitrox is running.
    /// </summary>
    public static class NitroxEnvironment
    {
        public static readonly Version Version = Assembly.GetAssembly(typeof(NitroxEnvironment)).GetName().Version;

        public enum Types
        {
            NORMAL,
            TESTING
        }

        private static bool hasSet;
        public static Types Type { get; private set; } = Types.NORMAL;

        public static bool IsNormal => Type == Types.NORMAL;

        public static bool IsTesting => Type == Types.TESTING;

        public static void Set(Types value)
        {
            if (hasSet)
            {
                throw new Exception("Enviroment type can only be set once.");
            }

            Type = value;
            hasSet = true;
        }
    }
}
