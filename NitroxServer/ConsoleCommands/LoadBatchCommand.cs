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
            addParameter(TypeInt.Get, "x", true);
            addParameter(TypeInt.Get, "y", true);
            addParameter(TypeInt.Get, "z", true);
        }

        protected override void Perform(Optional<Player> sender)
        {
            Int3 batchId = new Int3(readArgAt(0), readArgAt(1), readArgAt(2));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessageToBoth(sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
