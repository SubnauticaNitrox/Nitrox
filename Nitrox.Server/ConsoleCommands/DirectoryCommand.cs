using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Server.ConsoleCommands.Abstract;

namespace Nitrox.Server.ConsoleCommands
{
    internal sealed class DirectoryCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "dir" };

        public DirectoryCommand() : base("directory", Perms.CONSOLE, "Opens the directory of the main program")
        {
        }

        protected override void Execute(CallArgs args)
        {
            string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (!Directory.Exists(dir))
            {
                Log.Error($"Unable to open Nitrox directory '{dir}' because it does not exist.");
                return;
            }

            Process.Start(dir);
        }
    }
}
