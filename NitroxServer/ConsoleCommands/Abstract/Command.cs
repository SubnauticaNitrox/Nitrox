using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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
        public PermsFlag Flags { get; }
        public bool AllowedArgOverflow { get; set; }
        public List<IParameter<object>> Parameters { get; }

        protected Command(string name, Perms perms, PermsFlag flag, string description) : this(name, perms, description)
        {
            Flags = flag;
        }

        protected Command(string name, Perms perms, string description)
        {
            Validate.NotNull(name);

            Name = name;
            Flags = PermsFlag.NONE;
            RequiredPermLevel = perms;
            AllowedArgOverflow = false;
            Aliases = Array.Empty<string>();
            Parameters = new List<IParameter<object>>();
            Description = string.IsNullOrEmpty(description) ? "No description provided" : description;
        }

        protected abstract void Execute(CallArgs args);

        public void TryExecute(Optional<Player> sender, string[] args)
        {
            if (args.Length < required)
            {
                SendMessage(sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                return;
            }

            if (!AllowedArgOverflow && args.Length > optional + required)
            {
                SendMessage(sender, $"Error: Too many Parameters\nUsage: {ToHelpText(false, true)}");
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

        public bool CanExecute(Perms treshold)
        {
            return RequiredPermLevel <= treshold;
        }

        public string ToHelpText(bool singleCommand, bool cropText = false)
        {
            StringBuilder cmd = new(Name);

            if (Aliases.Any())
            {
                cmd.AppendFormat("/{0}", string.Join("/", Aliases));
            }

            cmd.AppendFormat(" {0}", string.Join(" ", Parameters));

            if (singleCommand)
            {
                string parameterPreText = Parameters.Count == 0 ? "" : Environment.NewLine;
                string parameterText = $"{parameterPreText}{string.Join("\n", Parameters.Select(p => $"{p,-47} - {p.GetDescription()}"))}";

                return cropText ? $"{cmd}" : $"{cmd,-32} - {Description} {parameterText}";
            }
            return cropText ? $"{cmd}" : $"{cmd,-32} - {Description}";
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

        /// <summary>
        /// Send a message to an existing player
        /// </summary>
        public static void SendMessageToPlayer(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            }
        }

        /// <summary>
        /// Send a message to an existing player and logs it in the console
        /// </summary>
        public static void SendMessage(Optional<Player> player, string message)
        {
            SendMessageToPlayer(player, message);
            if (!player.HasValue)
            {
                Log.Info(message);
            }
        }

        /// <summary>
        /// Send a message to all connected players
        /// </summary>
        public static void SendMessageToAllPlayers(string message)
        {
            PlayerManager playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, message));
            Log.Info($"[BROADCAST] {message}");
        }
    }
}
