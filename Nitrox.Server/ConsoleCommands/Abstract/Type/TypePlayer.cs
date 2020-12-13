using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.ConsoleCommands.Abstract.Type
{
    public class TypePlayer : Parameter<Player>
    {
        private static readonly PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();

        public TypePlayer(string name, bool required) : base(name, required)
        {
            Validate.NotNull(playerManager, "PlayerManager can't be null to resolve the command");
        }

        public override bool IsValid(string arg)
        {
            return playerManager.TryGetPlayerByName(arg, out Player player);
        }

        public override Player Read(string arg)
        {
            Validate.IsTrue(playerManager.TryGetPlayerByName(arg, out Player player), "Player not found");
            return player;
        }
    }
}
