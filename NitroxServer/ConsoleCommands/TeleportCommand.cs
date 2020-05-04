using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using UnityEngine;

namespace NitroxServer.ConsoleCommands
{
    internal class TeleportCommand : Command
    {
        public TeleportCommand() : base("tp", Perms.ADMIN, "Teleports you on a location")
        {
            AddAlias("teleport");
            AddParameter(new TypeInt("x", true));
            AddParameter(new TypeInt("y", true));
            AddParameter(new TypeInt("z", true));
        }

        protected override void Execute(CallArgs args)
        {
            Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");

            Vector3 position = new Vector3(args.Get<int>(0), args.Get<int>(1), args.Get<int>(2));
            args.Sender.Value.Teleport(position);

            SendMessage(args.Sender, $"Teleported to {position}");
        }
    }
}
