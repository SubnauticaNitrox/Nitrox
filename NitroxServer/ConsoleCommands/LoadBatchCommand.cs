using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    /// <summary>
    /// Mainly used for testing but can be used to pregen the world
    /// </summary>
    class LoadBatchCommand : Command
    {
        private BatchEntitySpawner batchEntitySpawner;

        public LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : base("loadbatch", Perms.CONSOLE, "<x> <y> <z>", "Load batch x y z")
        {
            this.batchEntitySpawner = batchEntitySpawner;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            batchEntitySpawner.LoadUnspawnedEntities(new NitroxModel.DataStructures.Int3(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2])));
        }

        public override bool VerifyArgs(string[] args)
        {
            int _;
            return args.Length == 3 && int.TryParse(args[0], out _) && int.TryParse(args[1], out _) && int.TryParse(args[2], out _);
        }
    }
}
