using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        private int optional;

        private int required;
        public string Name { get; }
        public string Description { get; }
        public List<string> Alias { get; }
        private string[] Args { get; set; }
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

        protected abstract void Perform(Optional<Player> sender);

        public void TryPerform(string[] args, Optional<Player> sender)
        {
            if (args.Length < required)
            {
                SendMessage(sender, $"Error: Invalid Parameters\nUsage: {ToHelpText().Trim()}");
                return;
            }

            if (!AllowedArgOverflow && args.Length > optional + required)
            {
                SendMessage(sender, $"Error: Too many Parameters\nUsage: {ToHelpText().Trim()}");
                return;
            }

            try
            {
                Args = args;
                Perform(sender);
            }
            catch (IllegalArgumentException e)
            {
                SendMessage(sender, $"Error: {e.Message}");
            }
            catch (Exception e)
            {
                Log.Error("Fatal error while trying to execute the command", e);
            }

            Args = null;
        }

        public bool IsValidArgAt(int index)
        {
            return index < Args.Length && index >= 0 && Args.Length != 0;
        }

        public string GetArgOverflow(int offset = 0)
        {
            if (Args?.Length != 0)
            {
                return string.Join(" ", Args.Skip(required + offset));
            }

            return string.Empty;
        }

        public string ReadArgAt(int index)
        {
            return ReadArgAt<string>(index);
        }

        public T ReadArgAt<T>(int index)
        {
            IParameter<object> param = Parameters[index];
            string arg = !IsValidArgAt(index) ? null : Args[index];
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

        public string ToHelpText()
        {
            StringBuilder cmd = new StringBuilder(Name);

            if (Alias?.Count > 0)
            {
                cmd.AppendFormat("/{0}", string.Join("/", Alias));
            }

            cmd.AppendFormat(" {0}", string.Join(" ", Parameters));

            return $"{cmd,-32} - {Description}";
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
        ///     Send a message to an existing player, otherwise logs it in the console
        /// </summary>
        public void SendMessage(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            }
            else
            {
                Log.Info(message);
            }
        }

        /// <summary>
        ///     Send a message to both console and an existing player
        /// </summary>
        public void SendMessageToBoth(Optional<Player> player, string message)
        {
            Log.Info(message);
            SendMessageToPlayer(player, message);
        }

        protected void AddParameter<T>(T param) where T : IParameter<object>
        {
            if (param.Equals(default(T)))
            {
                throw new ArgumentException("Parameter should not be null.", nameof(param));
            }
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
    }
}
