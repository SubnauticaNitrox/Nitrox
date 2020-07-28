using System;

namespace NitroxModel.Helper
{
    public static class NitroxEnvironment
    {
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
                throw new NitroxEnvironmentException("Enviroment type can only be set once.");
            }

            Type = value;
            hasSet = true;
        }
    }

    [Serializable]
    public class NitroxEnvironmentException : Exception
    {
        public NitroxEnvironmentException() { }
        public NitroxEnvironmentException(string message) : base(message) { }
        public NitroxEnvironmentException(string message, Exception inner) : base(message, inner) { }
        protected NitroxEnvironmentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
