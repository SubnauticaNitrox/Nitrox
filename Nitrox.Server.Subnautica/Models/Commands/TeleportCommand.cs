using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class TeleportCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "tp" };

        public TeleportCommand() : base("teleport", Perms.MODERATOR, PermsFlag.NO_CONSOLE, "Teleports you on a specific location")
        {
            AddParameter(new TypeInt("x", true, "x coordinate"));
            AddParameter(new TypeInt("y", true, "y coordinate"));
            AddParameter(new TypeInt("z", true, "z coordinate"));
        }

        protected override void Execute(CallArgs args)
        {
            NitroxVector3 position = new(args.Get<int>(0), args.Get<int>(1), args.Get<int>(2));
            args.Sender.Value.Teleport(position, Optional.Empty);

            SendMessage(args.Sender, $"Teleported to {position}");
        }
    }
}
