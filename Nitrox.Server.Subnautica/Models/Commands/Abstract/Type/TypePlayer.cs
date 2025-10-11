using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.Abstract.Type
{
    public class TypePlayer : Parameter<Player>
    {
        private static readonly PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();

        public TypePlayer(string name, bool required, string description) : base(name, required, description)
        {
            Validate.NotNull(playerManager, "PlayerManager can't be null to resolve the command");
        }

        public override bool IsValid(string arg)
        {
            return playerManager.TryGetPlayerByName(arg, out _);
        }

        public override Player Read(string arg)
        {
            Validate.IsTrue(playerManager.TryGetPlayerByName(arg, out Player player), "Player not found");
            return player;
        }
    }
}
