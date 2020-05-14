using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypePlayer : Parameter<Player>
    {
        private static readonly PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
        private readonly bool connectionRequired;

        public TypePlayer(string name, bool required, bool connectionRequired = true) : base(name, required)
        {
            Validate.NotNull(playerManager, "PlayerManager can't be null to resolve the command");
            this.connectionRequired = connectionRequired;
        }

        public override bool IsValid(string arg)
        {
            Player player;
            if (connectionRequired)
            {
                return playerManager.TryGetConnectedPlayerByName(arg, out player);
            }
            return playerManager.TryGetPlayerByName(arg, out player);
        }

        public override Player Read(string arg)
        {
            Player player;
            if (connectionRequired)
            {
                Validate.IsTrue(playerManager.TryGetConnectedPlayerByName(arg, out player), "Player not found");
            }
            else
            {
                Validate.IsTrue(playerManager.TryGetPlayerByName(arg, out player), "Player not found");
            }
            return player;
        }
    }
}
