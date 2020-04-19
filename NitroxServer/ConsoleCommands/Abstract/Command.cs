using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.Logger;
using NitroxModel.Helper;
using System.Text;
using System;
using NitroxServer.Exceptions;

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

        public string[] Args { get; private set; }

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

        public abstract void Perform(Optional<Player> sender);

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
                return;
            }

            Args = args;
            Perform(sender);
            Args = null;
        }

        public string getArgAt(int index)
        {
            if (index > Args.Length || index < 0)
            {
                return null;
            }

            return Args[index];
        }

        public dynamic readArgAt(int index)
        {
            try
            {
                dynamic param = Parameters[index];
                string arg = getArgAt(index);

                if (arg == null)
                {
                    Log.Error($"Argument at index {index} doesn't exist");
                    return null;
                }

                dynamic typeabstract = param.Type;

                return typeabstract?.read(arg);
            }
            catch (IllegalArgumentException ex)
            {
                Log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Dynamic reading arg can't resolve the correct type, you are on your own", ex);
            }

            return null;
        }

        protected void addParameter<T>(TypeAbstract<T> type, string name, bool isRequired)
        {
            addParameter<T>(new Parameter<T>(type, name, isRequired));
        }

        protected void addParameter<T>(IParameter param)
        {
            Validate.NotNull(param);
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

        public string GetSenderName(Optional<Player> player)
        {
            return player.HasValue ? player.Value.Name : "SERVER";
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
            }
            else
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
