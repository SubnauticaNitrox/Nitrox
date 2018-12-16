using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public string[] Args { get; protected set; }
        public string Name { get; protected set; }
        public string[] Alias { get; protected set; }

        /// <summary>
        /// Runs your command
        /// </summary>
        /// <param name="args">
        /// Arguments passed to your command
        /// </param>
        public abstract void RunCommand(string[] args);
        public abstract bool VerifyArgs(string[] args);
    }
}
