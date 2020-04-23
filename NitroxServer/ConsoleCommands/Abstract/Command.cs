using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        private readonly List<string> aliases;
        private int optional;
        private int required;

        protected Command(string name, Perms perm, string description, bool allowedArgOveflow = false)
        {
            Validate.NotNull(name);

            Name = name;
            RequiredPermLevel = perm;
            aliases = new List<string>();
            Parameters = new List<IParameter<object>>();
            AllowedArgOverflow = allowedArgOveflow;
            Description = string.IsNullOrEmpty(description) ? "No description provided" : description;
        }

        public string Name { get; }
        public string Description { get; }
        public Perms RequiredPermLevel { get; }
        public bool AllowedArgOverflow { get; }

        public ReadOnlyCollection<string> Aliases => aliases.AsReadOnly();

        private List<IParameter<object>> Parameters { get; }

        protected abstract void Execute(CallArgs args);

        public void TryExecute(Optional<Player> sender, string[] args)
        {
            if (args.Length < required)
            {
                SendMessage(sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(true)}");
                return;
            }

            if (!AllowedArgOverflow && args.Length > optional + required)
            {
                SendMessage(sender, $"Error: Too many Parameters\nUsage: {ToHelpText(true)}");
                return;
            }

            try
            {
                Execute(new CallArgs(this, sender, args));
            }
            catch (ArgumentException e)
            {
                SendMessage(sender, $"Error: {e.Message}");
            }
            catch (Exception e)
            {
                Log.Error("Fatal error while trying to execute the command", e);
            }
        }

        public string ToHelpText(bool cropText = false)
        {
            StringBuilder cmd = new StringBuilder(Name);
            if (Aliases?.Count > 0)
            {
                cmd.AppendFormat("/{0}", string.Join("/", Aliases));
            }
            cmd.AppendFormat(" {0}", string.Join(" ", Parameters));
            return cropText ? $"{cmd}" : $"{cmd,-32} - {Description}";
        }

        /// <summary>
        ///     Send a message to an existing player
        /// </summary>
        public void SendMessageToPlayer(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            }
        }

        /// <summary>
        ///     Send a message to an existing player and logs it in the console
        /// </summary>
        public void SendMessage(Optional<Player> player, string message)
        {
            SendMessageToPlayer(player, message);
            Log.Info(message);
        }

        protected void AddParameter<T>(T param) where T : IParameter<object>
        {
            Validate.NotNull(param as object);

            Parameters.Add(param);
            if (param.IsRequired)
            {
                required++;
            }
            else
            {
                optional++;
            }
        }

        protected void AddAlias(params string[] alias)
        {
            aliases.AddRange(alias);
        }

        protected void AddAlias(string alias)
        {
            aliases.Add(alias);
        }

        public class CallArgs
        {
            public CallArgs(Command command, Optional<Player> sender, string[] args)
            {
                Command = command;
                Sender = sender;
                Args = args;
            }

            public Command Command { get; }
            public string[] Args { get; }
            public Optional<Player> Sender { get; }

            public string SenderName => Sender.HasValue ? Sender.Value.Name : "SERVER";

            public bool Valid(int index)
            {
                return index < Args.Length && index >= 0 && Args.Length != 0;
            }

            public string Get(int index)
            {
                return Get<string>(index);
            }

            public string GetTillEnd(int startIndex = 0)
            {
                // TODO: Proper argument capture/parse instead of this argument join hack
                if (Args?.Length > 0)
                {
                    return string.Join(" ", Args.Skip(startIndex));
                }

                return string.Empty;
            }

            public T Get<T>(int index)
            {
                IParameter<object> param = Command.Parameters[index];
                string arg = Valid(index) ? Args[index] : null;

                if (arg == null)
                {
                    return default(T);
                }
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)arg;
                }

                return (T)param.Read(arg);
            }
        }
    }
}
