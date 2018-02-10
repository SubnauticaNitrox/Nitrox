using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole.Events
{
    public class CommandEventArgs : EventArgs
    {
        private string handlerMessage;

        /// <summary>
        /// Optional category of the command. Defaults to 'nitrox'.
        /// </summary>
        public string Category { get; }
        public string Command { get; }

        /// <summary>
        /// The full command including parts that are default and not given directly as text as with <see cref="Value"/>.
        /// </summary>
        public string VerboseValue { get; }

        /// <summary>
        /// The command as it was received.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Instance of a class that has the command handler or null if method is on a static class.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// The method that handles the command.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Arguments as they were parsed from input. Use <see cref="Get{T}"/> for more convenience.
        /// </summary>
        public ReadOnlyCollection<CommandArgInput> Arguments { get; }

        public bool HasError { get; private set; }

        public string HandlerMessage
        {
            get { return handlerMessage ?? ""; }
            set { handlerMessage = value; }
        }

        public CommandEventArgs(CommandCandidateEventArgs candidate)
        {
            Validate.NotNull(candidate);

            if (!candidate.IsValid())
            {
                throw new ArgumentException("Cannot convert an invalid candidate command into a command.", nameof(candidate));
            }

            Category = candidate.Category;
            Command = candidate.Command;
            Value = candidate.Value;
            VerboseValue = candidate.VerboseValue;
            Instance = candidate.Instance;
            Method = candidate.Method;
            Arguments = candidate.Arguments;
        }

        public Optional<T> Get<T>(string argName)
        {
            Validate.NotNullOrWhitespace(argName);

            Optional<CommandDatabase.CommandsStore> store = NitroxConsole.Main.GetStore(Category, Command);
            if (store.IsEmpty())
            {
                return Optional<T>.Empty();
            }

            Optional<CommandDatabase.CommandEntry> handler = store.Get().GetHandler(Arguments);
            if (handler.IsEmpty())
            {
                return Optional<T>.Empty();
            }

            ReadOnlyCollection<NitroxCommandArgAttribute> handlerArgs = handler.Get().Args;

            NitroxCommandArgAttribute cmdArg = handlerArgs.FirstOrDefault(hA => hA.HasName(argName));
            if (cmdArg == null)
            {
                return Optional<T>.Empty();
            }

            return Optional<T>.OfNullable((T)Arguments.FirstOrDefault(a => cmdArg.HasName(a.Name))?.Value);
        }

        public void Error(string message = null)
        {
            HasError = true;
            HandlerMessage = string.IsNullOrEmpty(message) ? HandlerMessage : message;
        }
    }
}

