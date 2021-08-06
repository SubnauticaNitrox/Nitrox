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
            AddParameter(new TypePlayer("name", true, "The players name"));
        }

        protected override void Execute(CallArgs args)
        {
            Player player = args.Get<Player>(0);

            StringBuilder builder = new($"==== {player.Name} ====\n");
            builder.AppendLine($"ID: {player.Id}");
            builder.AppendLine($"Role: {player.Permissions}");
            builder.AppendLine($"Position: {player.Position.X}, {player.Position.Y}, {player.Position.Z}");
            builder.AppendLine($"Oxygen: {player.Stats.Oxygen}/{player.Stats.MaxOxygen}");
            builder.AppendLine($"Food: {player.Stats.Food}");
            builder.AppendLine($"Water: {player.Stats.Water}");
            builder.AppendLine($"Infection: {player.Stats.InfectionAmount}");

            SendMessage(args.Sender, builder.ToString());
        }
    }
}
