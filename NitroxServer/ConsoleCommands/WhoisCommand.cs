using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WhoisCommand : Command
    {
        public WhoisCommand() : base("whois", Perms.PLAYER, "Shows informations over a player")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(CallArgs args)
        {
            Player player = args.Get<Player>(0);

            StringBuilder info = new StringBuilder($"==== {player.Name} ====\n");
            info.AppendLine($"ID: {player.Id}");
            info.AppendLine($"Role: {player.Permissions}");
            info.AppendLine($"Position: {player.Position.x}, {player.Position.y}, {player.Position.z}");
            info.AppendLine($"Oxygen: {player.Stats.Oxygen}/{player.Stats.MaxOxygen}");
            info.AppendLine($"Food: {player.Stats.Food}");
            info.AppendLine($"Water: {player.Stats.Water}");
            info.AppendLine($"Infection: {player.Stats.InfectionAmount}");

            SendMessage(args.Sender, info.ToString());
        }
    }
}
