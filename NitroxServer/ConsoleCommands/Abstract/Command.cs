using NitroxModel.Helper;
using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Logger;
using NitroxModel.Core;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public string Name { get; }
        public string Description { get; }
        public List<string> Alias { get; }
        public List<IParameter> Parameters { get; }
        public Perms RequiredPermLevel { get; }
        public bool AllowedArgOverflow { get; }

        private int required = 0;
        private int optional = 0;

        public Command(string name, Perms perm, string description, bool allowedArgOveflow = false)
        {
            Validate.NotNull(name);

            Name = name;
            RequiredPermLevel = perm;
            Alias = new List<string>();
            Parameters = new List<IParameter>();
            AllowedArgOverflow = allowedArgOveflow;
            Description = string.IsNullOrEmpty(description) ? "No description provided" : description;
        }

        public abstract void Perform(string[] args, Optional<Player> sender);

        public void TryPerform(string[] args, Optional<Player> sender)
        {
            if (args.Length < required)
            {
                SendMessage(sender, $"Error: Invalid Parameters\nUsage: {ToHelpText()}");
                return;
            }

            if (!AllowedArgOverflow && args.Length > optional + required)
            {
                SendMessage(sender, $"Error: Too much Parameters\nUsage: {ToHelpText()}");
            }

            Perform(args, sender);
        }

        protected void addParameter<T>(T defaultValue, TypeAbstract<T> type, string name, bool isRequired)
        {
            addParameter<T>(new Parameter<T>(defaultValue, type, name, isRequired));
        }

        protected void addParameter<T>(IParameter param)
        {
            Validate.NotNull(param);
            Parameters.Add(param);
            
            if (param.IsRequired)
            {
                required++;
            } else
            {
                optional++;
            }
        }

        protected void addAlias(params string[] alias)
        {
            Alias.AddRange(alias);
        }

        protected void addAlias(string alias)
        {
            Alias.Add(alias);
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

        /// <summary>
        /// Send a message to an existing player
        /// </summary>
        public void SendMessageToPlayer(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            }
        }

        /// <summary>
        /// Send a message to an existing player, otherwise logs it in the console
        /// </summary>
        public void SendMessage(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            } else
            {
                Log.Info(message);
            }
        }

        /// <summary>
        /// Send a message to both console and an existing player
        /// </summary>
        public void SendMessageToBoth(Optional<Player> player, string message)
        {
            Log.Info(message);
            SendMessageToPlayer(player, message);
        }
    }
}
