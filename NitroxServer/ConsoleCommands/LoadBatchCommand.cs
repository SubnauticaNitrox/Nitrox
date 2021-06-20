using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.ConsoleCommands
{
    internal class LoadBatchCommand : Command
    {
        private readonly BatchEntitySpawner batchEntitySpawner;

        public LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : base("loadbatch", Perms.ANY, "Loads entities at x y z")
        {
            this.batchEntitySpawner = batchEntitySpawner;
            AddParameter(new TypeInt("x", true));
            AddParameter(new TypeInt("y", true));
            AddParameter(new TypeInt("z", true));
        }

        protected override void Execute(CallArgs args)
        {
            NitroxInt3 batchId = new(args.Get<int>(0), args.Get<int>(1), args.Get<int>(2));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessage(args.Sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
