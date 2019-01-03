using System;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public string Args { get; protected set; }
        public string Name { get; protected set; }
        public string[] Alias { get; protected set; }

        protected Command(string name, string args = null, string[] alias = null)
        {
            Name = name;
            if (args == null)
            {
                args = name;
            }

            Args = args;
            Alias = alias ?? new string[0];
        }

        /// <summary>
        ///     Runs your command
        /// </summary>
        /// <param name="args">
        ///     Arguments passed to your command
        /// </param>
        public abstract void RunCommand(string[] args);

        public abstract bool VerifyArgs(string[] args);

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Alias)}: {string.Join(", ", Alias)}";
        }
    }
}
