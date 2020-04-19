using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxServer.Exceptions;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypePlayer : TypeAbstract<Player>
    {
        #region Singleton
        private static TypePlayer get;

        public static TypePlayer Get
        {
            get
            {
                return get ?? (get = new TypePlayer());
            }
        }
        #endregion

        private readonly PlayerManager pm;

        public TypePlayer()
        {
            Validate.NotNull(pm = NitroxServiceLocator.LocateService<PlayerManager>(), "PlayerManager can't be null to resolve the command");
        }

        public override bool IsValid(string arg)
        {
            Player _;
            return pm.TryGetPlayerByName(arg, out _);
        }

        public override Player Read(string arg)
        {
            Player _;

            if (!pm.TryGetPlayerByName(arg, out _))
            {
                throw new IllegalArgumentException("Player not found");
            }

            return _;
        }
    }
}
