using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract partial class Command
    {
        private int optional, required;

        public virtual IEnumerable<string> Aliases { get; }

        public string Name { get; }
        public string Description { get; }
        public Perms RequiredPermLevel { get; }
        public bool AllowedArgOverflow { get; }
        public List<IParameter<object>> Parameters { get; }

        protected Command(string name, Perms perm, string description, bool allowedArgOveflow = false)
        {
            Validate.NotNull(name);

            Name = name;
            RequiredPermLevel = perm;
            Aliases = Array.Empty<string>();
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
            catch (ArgumentException ex)
            {
                SendMessage(sender, $"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fatal error while trying to execute the command");
            }
        }

        public string ToHelpText(bool cropText = false)
        {
            StringBuilder cmd = new(Name);

            if (Aliases.Any())
            {
                cmd.AppendFormat("/{0}", string.Join("/", Aliases));
            }

            cmd.AppendFormat(" {0}", string.Join(" ", Parameters));
            return cropText ? $"{cmd}" : $"{cmd,-32} - {Description}";
        }

        public bool ContainsFlag(Perms flag)
        {
            return (RequiredPermLevel & flag) == flag;
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
        /// Send a message to an existing player and logs it in the console
        /// </summary>
        public void SendMessage(Optional<Player> player, string message)
        {
            SendMessageToPlayer(player, message);
            Log.Info(message);
        }

        /// <summary>
        /// Send a message to all connected players
        /// </summary>
        public void SendMessageToAllPlayers(string message)
        {
            PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, message));
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
    }
}
