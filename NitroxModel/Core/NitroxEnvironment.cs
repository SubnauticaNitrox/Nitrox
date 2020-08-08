using System;

namespace NitroxModel.Helper
{
    public static class NitroxEnvironment
    {
        private static bool hasBeenSet;
        private static NitroxEnvironmentTypes type = NitroxEnvironmentTypes.NORMAL;

        public static bool IsNormal => type == NitroxEnvironmentTypes.NORMAL;
        public static bool IsTesting => type == NitroxEnvironmentTypes.TESTING;

        public static void Set(NitroxEnvironmentTypes value)
        {
            if (hasBeenSet)
            {
                throw new InvalidOperationException("NitroxEnvironmentTypes can only be set once.");
            }

            type = value;
            hasBeenSet = true;
        }
    }

    public enum NitroxEnvironmentTypes
    {
        NORMAL,
        TESTING
    }
}
