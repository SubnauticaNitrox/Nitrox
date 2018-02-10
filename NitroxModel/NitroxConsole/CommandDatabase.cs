using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.NitroxConsole.Events;
using NitroxModel.NitroxConsole.Typing;

namespace NitroxModel.NitroxConsole
{
    /// <summary>
    /// Stores commands and can invoke commands directly.
    /// </summary>
    public class CommandDatabase
    {
        private readonly Dictionary<CommandIdentity, CommandsStore> Commands = new Dictionary<CommandIdentity, CommandsStore>();
        /// <summary>
        /// Keeps track of the entered commands.
        /// </summary>

        public CommandEntry Add(Action<CommandEventArgs> handler)
        {
            CommandEntry entry = CommandEntry.From(handler);
            AddEntry(entry);

            return entry;
        }

        [Obsolete("Prefer using Add(handler) over adding commands directly.")]
        internal CommandEntry Add(string category, string command, Action handler)
        {
            CommandEntry entry = CommandEntry.From(category, command, handler);
            AddEntry(entry, true);
            
            return entry;
        }

        private void AddEntry(CommandEntry entry, bool silentlyFail = false)
        {
            if (!Commands.ContainsKey(entry.Command.Identity))
            {
                Commands.Add(entry.Command.Identity, new CommandsStore(entry.Command.Identity));
            }

            CommandsStore store = Commands[entry.Command.Identity];
            if (store.Contains(entry))
            {
                if (!silentlyFail)
                {
                    throw new NotSupportedException($"Command handler already exists in database for command: {entry.Instance.GetType().FullName}.{entry.Method.Name}");
                }

                return;
            }
            store.Add(entry, silentlyFail);
        }

        public Optional<CommandsStore> GetStore(CommandIdentity identity)
        {
            CommandsStore store;
            Commands.TryGetValue(identity, out store);
            return Optional<CommandsStore>.OfNullable(store);
        }

        public bool TryInvokeHandler(CommandEventArgs e)
        {
            return GetStore(new CommandIdentity(e.Category, e.Command)).Then(s => s.GetHandler(e.Arguments).Then(ce => ce.EventHandler(this, e)));
        }

        public class CommandsStore
        {
            public CommandIdentity Identity { get; }
            protected List<CommandEntry> Entries = new List<CommandEntry>();

            public CommandsStore(CommandIdentity identity)
            {
                Validate.NotNull(identity);

                Identity = identity;
            }

            protected internal void Add(CommandEntry entry, bool silentlyFail = false)
            {
                if (Entries.Contains(entry))
                {
                    if (!silentlyFail)
                    {
                        throw new NotSupportedException("Command entry already exists in store.");
                    }

                    return;
                }

                CommandEntry conflicting = Entries.FirstOrDefault(e => e.IsSameHandler(entry));
                if (conflicting != null)
                {
                    if (!silentlyFail)
                    {
                        throw new NotSupportedException($"Another command entry handles the same command. Given: {entry}, Conflicting: {conflicting}");
                    }

                    return;
                }

                Entries.Add(entry);
            }

            /// <summary>
            /// Get all the registered command entries in this store.
            /// </summary>
            /// <returns>Commands with the same <see cref="CommandIdentity"/>.</returns>
            public IEnumerable<CommandEntry> GetEntries()
            {
                return Entries;
            }

            /// <summary>
            /// Compares the given arguments with all the arguments in the store. Doesn't compare <see cref="CommandIdentity"/> because it's expected to be equal to the current store.
            /// </summary>
            /// <param name="given">Arguments to find a handler for.</param>
            /// <returns>Registered command handler for the command arguments.</returns>
            public Optional<CommandEntry> GetHandler(IEnumerable<CommandArgInput> given)
            {
                CommandEntry bestMatch = Entries.FirstOrDefault(entry => entry.Args.Where(a => a.Required).All(a => given.Any(g => a.HasName(g.Name))));
                return Optional<CommandEntry>.OfNullable(bestMatch);
            }

            public bool Contains(CommandEntry entry)
            {
                if (entry == null)
                {
                    return false;
                }

                return Entries.Contains(entry);
            }
        }

        /// <summary>
        ///     Used by <see cref="CommandDatabase" /> to store a new registered command entry.
        /// </summary>
        public class CommandEntry : IEquatable<CommandEntry>
        {
            private CommandEntry(string category, string command, Action handler)
            {
                Args = new ReadOnlyCollection<NitroxCommandArgAttribute>(new List<NitroxCommandArgAttribute>());
                Command = new NitroxCommandAttribute(category, command);
                Instance = handler.Target;
                Method = handler.Method;
                InternalDelegate = args => handler();
                EventHandler = (sender, e) => InternalDelegate(e);
            }

            private CommandEntry(NitroxCommandAttribute command, List<NitroxCommandArgAttribute> args, MethodInfo method, object instance = null)
            {
                Validate.NotNull(command);
                Validate.NotNull(args);
                Validate.NotNull(method);

                // Validate that there aren't any conflicting argument names.
                if (args.SelectMany(a => new[] {a.Name}.Concat(a.Aliases)).GroupBy(n => n).Any(g => g.Count() > 1))
                {
                    throw new DuplicateNameException(
                        $"Commands must not have conflicting argument names or aliases. Command: {command.Identity}, Command's method: {method.DeclaringType?.FullName}.{method.Name}");
                }

                Instance = instance;
                Command = command;
                Method = method;
                Args = new ReadOnlyCollection<NitroxCommandArgAttribute>(args);
                InternalDelegate = (Action<CommandEventArgs>)Delegate.CreateDelegate(typeof(Action<CommandEventArgs>), Instance, Method);
                EventHandler = (sender, e) => InternalDelegate(e);
            }

            /// <summary>
            ///     Instance of a class that has the command handler.
            /// </summary>
            public object Instance { get; }

            public NitroxCommandAttribute Command { get; }
            public ReadOnlyCollection<NitroxCommandArgAttribute> Args { get; }
            public MethodInfo Method { get; }
            public EventHandler<CommandEventArgs> EventHandler { get; }
            private Action<CommandEventArgs> InternalDelegate { get; }

            public static CommandEntry From(Action<CommandEventArgs> method)
            {
                Validate.NotNull(method);
                return From(method.Method, method.Target);
            }

            /// <summary>
            /// Creates a command registration entry from a method with an optional instance for istance methods.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="method"></param>
            /// <returns></returns>
            public static CommandEntry From(MethodInfo method, object instance = null)
            {
                Validate.NotNull(method);

                NitroxCommandAttribute cmd = method.GetCustomAttributes(typeof(NitroxCommandAttribute), false)
                    .OfType<NitroxCommandAttribute>().Single();
                List<NitroxCommandArgAttribute> args = method.GetCustomAttributes(typeof(NitroxCommandArgAttribute), false)
                    .OfType<NitroxCommandArgAttribute>().ToList();

                return new CommandEntry(cmd, args, method, instance);
            }

            public bool IsSameHandler(CommandEntry other)
            {
                if (other == null)
                {
                    return false;
                }

                return Command.Identity == other.Command.Identity && Args.All(a => other.Args.Any(otherA => otherA.IsSameCommandArg(a)));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as CommandEntry);
            }

            public bool Equals(CommandEntry other)
            {
                return other != null &&
                       EqualityComparer<object>.Default.Equals(Instance, other.Instance) &&
                       EqualityComparer<NitroxCommandAttribute>.Default.Equals(Command, other.Command) &&
                       EqualityComparer<ReadOnlyCollection<NitroxCommandArgAttribute>>.Default.Equals(Args, other.Args) &&
                       EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
            }

            public override int GetHashCode()
            {
                int hashCode = 217259050;
                hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Instance);
                hashCode = hashCode * -1521134295 + EqualityComparer<NitroxCommandAttribute>.Default.GetHashCode(Command);
                hashCode = hashCode * -1521134295 + EqualityComparer<ReadOnlyCollection<NitroxCommandArgAttribute>>.Default.GetHashCode(Args);
                hashCode = hashCode * -1521134295 + EqualityComparer<MethodInfo>.Default.GetHashCode(Method);
                return hashCode;
            }

            public static bool operator ==(CommandEntry entry1, CommandEntry entry2)
            {
                return EqualityComparer<CommandEntry>.Default.Equals(entry1, entry2);
            }

            public static bool operator !=(CommandEntry entry1, CommandEntry entry2)
            {
                return !(entry1 == entry2);
            }

            public override string ToString()
            {
                string argString = Args.Any() ? Args.Select(a => a.Name).Aggregate((a1, a2) => a1 + ", " + a2) : "";
                return $"{nameof(Command)}: {Command}{(argString != "" ? $", {nameof(Args)}: {argString}" : "")}";
            }

            internal static CommandEntry From(string category, string command, Action handler)
            {
                Validate.NotNullOrWhitespace(category);
                Validate.NotNullOrWhitespace(command);
                Validate.NotNull(handler);

                return new CommandEntry(category, command, handler);
            }
        }
    }
}
