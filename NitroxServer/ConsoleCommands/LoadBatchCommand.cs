using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;

namespace NitroxServer.ConsoleCommands
{
    /// <summary>
    /// Mainly used for testing but can be used to pregen the world
    /// </summary>
    internal class LoadBatchCommand : Command
    {
        private readonly BatchEntitySpawner batchEntitySpawner;

        public LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : base("loadbatch", Perms.CONSOLE, "Loads entities at x y z")
        {
            this.batchEntitySpawner = batchEntitySpawner;
            addParameter(0, TypeInt.Get, "x", true);
            addParameter(0, TypeInt.Get, "y", true);
            addParameter(0, TypeInt.Get, "z", true);
        }

        public override void Perform(string[] args, Optional<Player> sender)
        {
            Int3 batchId = new Int3(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessageToBoth(sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
