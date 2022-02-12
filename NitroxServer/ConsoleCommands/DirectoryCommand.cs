using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class DirectoryCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "dir" };

        public DirectoryCommand() : base("directory", Perms.CONSOLE, "Opens the current directory of the server")
        {
        }

        protected override void Execute(CallArgs args)
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            if (!Directory.Exists(path))
            {
                Log.ErrorSensitive("Unable to open Nitrox directory {path} because it does not exist", path);
                return;

            }

            Log.InfoSensitive("Opening directory {path}", path);
            Process.Start(path);
        }
    }
}
