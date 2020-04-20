using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;
using NitroxServer.ConsoleCommands.Abstract.Type;

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
            AddParameter(new TypeInt("x", true));
            AddParameter(new TypeInt("y", true));
            AddParameter(new TypeInt("z", true));
        }

        protected override void Execute(Optional<Player> sender)
        {
            Int3 batchId = new Int3(ReadArgAt<int>(0), ReadArgAt<int>(1), ReadArgAt<int>(2));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessageToBoth(sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
