using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            string path;
            try
            {
                path = NitroxUser.ExecutableRootPath;
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Failed to get location of server executable");
                return;
            }
            path = path.EndsWith(Path.DirectorySeparatorChar) ? path : $"{path}{Path.DirectorySeparatorChar}";
            if (!Directory.Exists(path))
            {
                logger.LogOpenDirectoryNotExists(path);
                return;
            }

            logger.LogOpenDirectory(path);
            using Process? proc = Process.Start(new ProcessStartInfo
            {
                FileName = path,
                Verb = "open",
                UseShellExecute = true
            });
        }
    }
}
