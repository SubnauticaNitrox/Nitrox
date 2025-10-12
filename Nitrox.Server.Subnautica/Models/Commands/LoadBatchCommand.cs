#if DEBUG
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

namespace Nitrox.Server.Subnautica.Models.Commands
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
