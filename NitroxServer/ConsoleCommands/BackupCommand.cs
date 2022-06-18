using System;
using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class BackupCommand : Command
    {
        public BackupCommand() : base("backup", Perms.MODERATOR, "Backs up the save")
        {
        }

        protected override void Execute(CallArgs args)
        {
            string saveDir = null;
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith(WorldManager.SavesFolderDir, StringComparison.OrdinalIgnoreCase) && Directory.Exists(arg))
                {
                    saveDir = arg;
                    break;
                }
            }

            Server.Instance.Save();
            WorldManager.BackupSave(saveDir);
            SendMessageToPlayer(args.Sender, "World has been backed up");
        }
    }
}
