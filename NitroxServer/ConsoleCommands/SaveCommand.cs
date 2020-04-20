using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.ADMIN, "Saves the map")
        {
            AddParameter(new TypeBoolean("on/off", false));
        }

        protected override void Execute(Optional<Player> sender)
        {
            if (IsValidArgAt(0))
            {
                bool? toggle = ReadArgAt<bool?>(0);

                if (toggle.HasValue)
                {
                    if (toggle.Value)
                    {
                        Server.Instance.EnablePeriodicSaving();
                        SendMessage(sender, "Enabled periodical saving");
                    }
                    else
                    {
                        Server.Instance.DisablePeriodicSaving();
                        SendMessage(sender, "Disabled periodical saving");
                    }
                }
            }
            else
            {
                Server.Instance.Save();
                SendMessageToPlayer(sender, "World saved");
            }
        }
    }
}
