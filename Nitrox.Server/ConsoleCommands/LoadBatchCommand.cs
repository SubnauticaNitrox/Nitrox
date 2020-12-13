using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.GameLogic.Entities.Spawning;

namespace Nitrox.Server.ConsoleCommands
{
    /// <summary>
    ///     Mainly used for testing but can be used to pregen the world
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

        protected override void Execute(CallArgs args)
        {
            NitroxInt3 batchId = new NitroxInt3(args.Get<int>(0), args.Get<int>(1), args.Get<int>(2));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            SendMessage(args.Sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }
    }
}
