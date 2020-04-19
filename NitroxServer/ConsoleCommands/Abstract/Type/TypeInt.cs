using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypeInt : TypeAbstract<int>
    {
        #region Singleton
        private static TypeInt get;

        public static TypeInt Get
        {
            get
            {
                return get ?? (get = new TypeInt());
            }
        }
        #endregion

        public override bool IsValid(string arg)
        {
            int _;
            return int.TryParse(arg, out _);
        }

        public override int Read(string arg)
        {
            int _;

            if (!int.TryParse(arg, out _))
            {
                throw new IllegalArgumentException("Invalid integer received");
            }

            return _;
        }
    }
}
