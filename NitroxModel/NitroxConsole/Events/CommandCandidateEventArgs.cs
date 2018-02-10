using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole.Events
{
    /// <summary>
    /// Decides whether a given string is a command or not.
    /// </summary>
    public class CommandCandidateEventArgs : EventArgs
    {
        /// <summary>
        /// Pattern to match 3 parts:
        ///  - Command category
        ///  - Command name
        ///  - Command arguments
        /// </summary>
        /// <remarks>
        /// https://regex101.com/r/J1E3ut/2
        /// </remarks>
        private static readonly Regex cmdRegex = new Regex(@"^\s*(?<cat>(?>[a-zA-Z][a-zA-Z0-9]*))?\s?(?<cmd>[a-zA-Z][a-zA-Z0-9]*)\s?(?<args>[\s\S]*)");

        /// <summary>
        /// Patern to match 1 part multiple times:
        ///  - Command name + command value[string/number]
        /// </summary>
        /// <remarks>
        /// https://regex101.com/r/IoD56p/3
        /// </remarks>
        private static readonly Regex argsRegex = new Regex(@"-(?<var>[a-z][a-z0-9]*)(?: (?!\s*\-)(?>(?<num>\d*\.?\d+)(?:\s|$)|['""]?(?<str>(?<!['""])\S+|[^'""]+(?=['""] *\-)|.*(?=['""]))['""]?))?");

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
        /// Instance of an object that has the command handler.
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// The method that handles the command.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// The message supplied when <see cref="IsValid()"/> is false.
        /// </summary>
        public string StateMessage { get; }

        /// <summary>
        /// The state for why the command was invalid.
        /// </summary>
        public CommandStates State { get; private set; }

        /// <summary>
        /// Arguments as they were parsed from input.
        /// </summary>
        public ReadOnlyCollection<CommandArgInput> Arguments { get; }

        public CommandCandidateEventArgs(string value)
        {
            Validate.NotNullOrWhitespace(value);

            List<CommandArgInput> argList = new List<CommandArgInput>();
            Arguments = new ReadOnlyCollection<CommandArgInput>(argList);
            
            // Text to command validation.
            Value = value;
            Match cmdMatch = cmdRegex.Match(value);
            if (!cmdMatch.Success)
            {
                // This is not a command..
                State = CommandStates.FAILEDPARSING;
                return;
            }

            MatchCollection argMatches = null;
            if (cmdMatch.Groups["args"].Success)
            {
                argMatches = argsRegex.Matches(cmdMatch.Groups["args"].Value);
                if (argMatches.OfType<Match>().GroupBy(m => m.Groups["var"].Value.ToLowerInvariant()).Any(g => g.Count() > 1))
                {
                    State = CommandStates.DUPLICATEARGUMENTS;
                    StateMessage = "Duplicate argument names found in command.";
                    return;
                }
            }
            
            string cat = cmdMatch.Groups["cat"].Value;
            Category = cat == "" ? NitroxConsole.DEFAULT_CATEGORY : cat.ToLowerInvariant();
            Command = cmdMatch.Groups["cmd"].Value.ToLowerInvariant();
            if (string.IsNullOrEmpty(cat))
            {
                VerboseValue = $"{Category} {Value}";
            }

            if (argMatches == null)
            {
                argMatches = argsRegex.Matches(cmdMatch.Groups["args"].Value);
            }
            foreach (Match argMatch in argMatches)
            {
                if (argMatch.Groups["num"].Success)
                {
                    argList.Add(new CommandArgInput(argMatch.Groups["var"].Value.ToLowerInvariant(), argMatch.Groups["num"].Value));
                }
                else if (argMatch.Groups["str"].Success)
                {
                    argList.Add(new CommandArgInput(argMatch.Groups["var"].Value.ToLowerInvariant(), argMatch.Groups["str"].Value));
                }
            }

            // Textual command to handler validation.
            Optional<CommandDatabase.CommandsStore> store = NitroxConsole.Main.GetStore(Category, Command);
            Optional<CommandDatabase.CommandEntry> entry;
            if (store.IsEmpty() || (entry = store.Get().GetHandler(Arguments)).IsEmpty())
            {
                State = CommandStates.NOCOMMANDHANDLER;
                StateMessage = "No command handler for command.";
                return;
            }

            Instance = entry.Get().Instance;
            Method = entry.Get().Method;

            State = CommandStates.VALID;
        }

        public bool IsValid() => State == CommandStates.VALID;

        public enum CommandStates
        {
            /// <summary>
            /// Set when the command is valid.
            /// </summary>
            VALID,

            /// <summary>
            /// Set when the command didn't match the expected command pattern.
            /// </summary>
            FAILEDPARSING,

            /// <summary>
            /// Set when the arguments of the given command have duplicates in them.
            /// </summary>
            DUPLICATEARGUMENTS,

            /// <summary>
            /// Set when a command format was parsed successfully but no handler was found. 
            /// </summary>
            NOCOMMANDHANDLER
        }
    }
}

