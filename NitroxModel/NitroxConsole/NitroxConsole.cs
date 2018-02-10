using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole.Events;
using NitroxModel.NitroxConsole.Typing;

namespace NitroxModel.NitroxConsole
{
    public sealed class NitroxConsole
    {
        private static NitroxConsole main;
        internal const string DEFAULT_CATEGORY = "nitrox";

        public static NitroxConsole Main
        {
            get { return main = main ?? new NitroxConsole(); }
            set { main = value; }
        }

        public event EventHandler<ConsoleInputEventArgs> InputReceived;
        public event EventHandler<CommandEventArgs> CommandReceived;
        public event EventHandler<CommandEventArgs> CommandProcessed;
        public event EventHandler<CommandCandidateEventArgs> InvalidCommandReceived;
        private CommandDatabase Database { get; }
        private CommandHistory History { get; }

        private NitroxConsole()
        {
            Database = new CommandDatabase();
            History = new CommandHistory();
        }

        public void Submit(string input)
        {
            Validate.NotNullOrWhitespace(input);

            CommandCandidateEventArgs candidate = new CommandCandidateEventArgs(input);
            if (!candidate.IsValid())
            {
                switch (candidate.State)
                {
                    case CommandCandidateEventArgs.CommandStates.FAILEDPARSING:
                        // Because the command failed to parse as a command it's being regarded as normal text.
                        OnInputReceived(new ConsoleInputEventArgs(input));
                        return;
                }

                OnInvalidCommandReceived(candidate);
                return;
            }

            CommandEventArgs command = new CommandEventArgs(candidate);
            OnCommandReceived(command); // Invokes the command handlers.
            OnCommandProcessed(command);
        }

        public void AddCommand(Action<CommandEventArgs> args)
        {
            Database.Add(args);
        }

        [Obsolete("Prefer using AddCommand(Action) over directly adding commands.")]
        public void AddCommandDirect(string category, string command, Action handler)
        {
            Database.Add(category, command, handler);
        }

        public Optional<CommandDatabase.CommandsStore> GetStore(CommandIdentity identity)
        {
            Validate.NotNull(identity);

            return Database.GetStore(identity);
        }

        public Optional<CommandDatabase.CommandsStore> GetStore(string category, string command)
        {
            return GetStore(new CommandIdentity(category, command));
        }

        public CommandEventArgs HistoryPrevious()
        {
            return History.Previous();
        }

        public CommandEventArgs HistoryNext()
        {
            return History.Next();
        }

        public Optional<CommandEventArgs> HistoryMoveToNow()
        {
            return History.Now();
        }

        public void HistoryMoveAfterNow()
        {
            History.AfterNow();
        }

        private void OnCommandProcessed(CommandEventArgs e)
        {
            CommandProcessed?.Invoke(this, e);
        }

        private void OnCommandReceived(CommandEventArgs e)
        {
            if (!Database.TryInvokeHandler(e))
            {
                throw new InvalidOperationException($"Handler for command '{e.VerboseValue}' was found during parse but not on command receive.");
            }
            History.Add(e);
            CommandReceived?.Invoke(this, e);
        }

        private void OnInvalidCommandReceived(CommandCandidateEventArgs e)
        {
            Log.Debug($"Invalid console command: {e.Value}, Error: {e.State}{(!string.IsNullOrEmpty(e.StateMessage) ? $", Message: {e.StateMessage}" : "")}");
            InvalidCommandReceived?.Invoke(this, e);
        }

        private void OnInputReceived(ConsoleInputEventArgs e)
        {
            Log.Debug($"Console input received: {e.Input}");
            InputReceived?.Invoke(this, e);
        }
    }
}

