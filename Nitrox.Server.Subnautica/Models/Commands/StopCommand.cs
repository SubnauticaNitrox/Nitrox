using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class StopCommand : Command
    {
        private readonly IHostApplicationLifetime lifetime;
        public override IEnumerable<string> Aliases { get; } = ["exit", "halt", "quit", "close"];

        public StopCommand(IHostApplicationLifetime lifetime) : base("stop", Perms.ADMIN, "Stops the server")
        {
            this.lifetime = lifetime;
        }

        protected override void Execute(CallArgs args)
        {
            lifetime.StopApplication();
        }
    }
}
