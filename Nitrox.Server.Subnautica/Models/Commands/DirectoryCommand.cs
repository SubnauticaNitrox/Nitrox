using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class DirectoryCommand : Command
    {
        private readonly ILogger<DirectoryCommand> logger;
        public override IEnumerable<string> Aliases { get; } = ["dir"];

        public DirectoryCommand(ILogger<DirectoryCommand> logger) : base("directory", Perms.HOST, "Opens the current directory of the server")
        {
            this.logger = logger;
        }

        protected override void Execute(CallArgs args)
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            if (!Directory.Exists(path))
            {
                logger.ZLogError($"Unable to open Nitrox directory {path.ToSensitive()} because it does not exist");
                return;
            }

            logger.ZLogInformation($"Opening directory {path.ToSensitive()}");
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "open" })?.Dispose();
        }
    }
}
