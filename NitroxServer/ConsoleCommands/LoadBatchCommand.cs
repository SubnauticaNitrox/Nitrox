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

        public LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : base("loadbatch", Perms.CONSOLE, "{x} {y} {z}", "Loads entities at x y z")
        {
            this.batchEntitySpawner = batchEntitySpawner;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            Int3 batchId = new Int3(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
            List<Entity> entities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

            Notify(sender, $"Loaded {entities.Count} entities from batch {batchId}");
        }

        public override bool VerifyArgs(string[] args)
        {
            int _;
            return args.Length == 3 && int.TryParse(args[0], out _) && int.TryParse(args[1], out _) && int.TryParse(args[2], out _);
        }
    }
}
