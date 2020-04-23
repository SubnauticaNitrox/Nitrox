using System;
using System.Collections.Generic;
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
        private int optional;
        private int required;

        public string Name { get; }
        public string Description { get; }
        public List<string> Alias { get; }
        public Perms RequiredPermLevel { get; }
        public bool AllowedArgOverflow { get; }
        public List<IParameter<object>> Parameters { get; }

        public Command(string name, Perms perm, string description, bool allowedArgOveflow = false)
        {
            Validate.NotNull(name);

            Name = name;
            RequiredPermLevel = perm;
            Alias = new List<string>();
            Parameters = new List<IParameter<object>>();
            AllowedArgOverflow = allowedArgOveflow;
            Description = string.IsNullOrEmpty(description) ? "No description provided" : description;
        }

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

        public string ToHelpText(bool cropped = false)
        {
            StringBuilder cmd = new StringBuilder(Name);

            if (Alias?.Count > 0)
            {
                cmd.AppendFormat("/{0}", string.Join("/", Alias));
            }

            cmd.AppendFormat(" {0}", string.Join(" ", Parameters));

            return cropped ? $"- {cmd}" : $"{cmd,-32} - {Description}";
        }

        public string GetSenderName(Optional<Player> player)
        {
            return player.HasValue ? player.Value.Name : "SERVER";
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
            Validate.IsFalse(param.Equals(default(T)), "Parameter shouldn't be null");

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
            Alias.AddRange(alias);
        }

        protected void AddAlias(string alias)
        {
            Alias.Add(alias);
        }

        public class CallArgs
        {
            public Command Command { get; }
            public string[] Args { get; }
            public Optional<Player> Sender { get; }

            public CallArgs(Command command, Optional<Player> sender, string[] args)
            {
                Command = command;
                Sender = sender;
                Args = args;
            }

            public bool IsValidArgAt(int index)
            {
                return index < Args.Length && index >= 0 && Args.Length != 0;
            }

            public string GetOverflow(int offset = 0)
            {
                if (Args?.Length > 0)
                {
                    return string.Join(" ", Args.Skip(Command.required + offset));
                }

                return string.Empty;
            }

            public string Get(int index)
            {
                return Get<string>(index);
            }

            public T Get<T>(int index)
            {
                IParameter<object> param = Command.Parameters[index];
                string arg = IsValidArgAt(index) ? Args[index] : null;

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)arg;
                }

                if (arg == null || param == null)
                {
                    return default(T);
                }

                return (T)param.Read(arg);
            }
        }
    }
}
