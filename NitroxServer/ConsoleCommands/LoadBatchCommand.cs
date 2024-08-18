#if DEBUG
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

        public LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : base("loadbatch", Perms.ADMIN, "Loads entities at x y z")
        {
            AddParameter(new TypeInt("x", true, "x coordinate"));
            AddParameter(new TypeInt("y", true, "y coordinate"));
            AddParameter(new TypeInt("z", true, "z coordinate"));

            this.batchEntitySpawner = batchEntitySpawner;
        }

        protected override void Execute(CallArgs args)
        {
            NitroxInt3 batchId = new(args.Get<int>(0), args.Get<int>(1), args.Get<int>(2));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessage(args.Sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
#endif
