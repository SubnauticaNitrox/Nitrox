using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypeString : TypeAbstract<string>
    {
        #region Singleton
        private static TypeString get;

        public static TypeString Get
        {
            get
            {
                return get ?? (get = new TypeString());
            }
        }
        #endregion

        public override bool IsValid(string arg) => !string.IsNullOrEmpty(arg);

        public override string Read(string arg)
        {
            if (!IsValid(arg))
            {
                throw new IllegalArgumentException("Received null/empty instead of a valid string");
            }

            return arg;
        }
    }
}
