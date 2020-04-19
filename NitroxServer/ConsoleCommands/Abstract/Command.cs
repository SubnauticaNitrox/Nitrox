using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections.Generic;
using NitroxServer.Exceptions;
using NitroxModel.Packets;
using NitroxModel.Logger;
using NitroxModel.Helper;
using System.Linq;
using System.Text;
using System;


namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public string Name { get; }
        public string Description { get; }
        public List<string> Alias { get; }
        private string[] Args { get; set; }
        public Perms RequiredPermLevel { get; }
        public bool AllowedArgOverflow { get; }
        public List<IParameter> Parameters { get; }

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
                SendMessage(sender, $"Error: Too much Parameters\nUsage: {ToHelpText().Trim()}");
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

        public bool IsValidArgAt(int index) => index < Args.Length && index >= 0 && Args.Length != 0;

        public string getArgAt(int index) => !IsValidArgAt(index) ? null : Args[index];

        public string getArgOverflow(int offset = 0)
        {
            if (Args?.Length != 0)
            {
                return string.Join(" ", Args.Skip(required + offset));
            }

            return string.Empty;
        }

        public dynamic readArgAt(int index)
        {
            try
            {
                dynamic param = Parameters[index];
                string arg = getArgAt(index);

                if (arg == null || param == null)
                {
                    Log.Error($"Index {index} doesn't exist for this command");
                    return null;
                }

                dynamic typeabstract = param.Type;

                return typeabstract?.read(arg);
            }
            catch (RuntimeBinderException ex)
            {
                Log.Error("Dynamic argument reading can't resolve the correct type, you are on your own", ex);
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
