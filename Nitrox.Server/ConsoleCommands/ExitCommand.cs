using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;

namespace Nitrox.Server.ConsoleCommands
{
    internal class ExitCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "exit", "halt", "quit" };

        public ExitCommand() : base("stop", Perms.ADMIN, "Stops the server")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Server.Instance.Stop();
        }
    }
}
