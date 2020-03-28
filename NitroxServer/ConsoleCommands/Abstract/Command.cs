﻿using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Logger;
using System.Text;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public string Name { get; protected set; }
        public string[] Alias { get; protected set; }
        public string Description { get; protected set; }
        public string ArgsDescription { get; protected set; }
        public Perms RequiredPermLevel { get; protected set; } = Perms.ADMIN;

        protected Command(string name, Perms requiredPermLevel) : this(name, requiredPermLevel, "", "", null)
        {
            RequiredPermLevel = requiredPermLevel;
            Name = name;
        }

        protected Command(string name, Perms requiredPermLevel, string argsDescription) : this(name, requiredPermLevel, argsDescription, "", null)
        {
            ArgsDescription = argsDescription;
            Name = name;
        }

        protected Command(string name, Perms requiredPermLevel, string argsDescription, string description) : this(name, requiredPermLevel, argsDescription, "", null)
        {
            ArgsDescription = argsDescription;
            Name = name;
            Description = description;
        }

        protected Command(string name, Perms requiredPermLevel, string argsDescription, string description, string[] alias)
        {
            Validate.NotNull(name);
            Validate.NotNull(argsDescription);

            Name = name;
            Description = string.IsNullOrEmpty(description) ? "No description" : description;
            ArgsDescription = argsDescription;
            RequiredPermLevel = requiredPermLevel;
            Alias = alias ?? new string[0];
        }

        public abstract void RunCommand(string[] args, Optional<Player> sender);

        public abstract bool VerifyArgs(string[] args);

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(ArgsDescription)}: {ArgsDescription}, {nameof(Alias)}: [{string.Join(", ", Alias)}]";
        }

        public string ToHelpText()
        {
            StringBuilder cmd = new StringBuilder(Name);

            if (Alias.Length > 0)
            {
                cmd.AppendFormat("/{0}", string.Join("/", Alias));
            }
            cmd.AppendFormat(" {0}", ArgsDescription);

            return $"{cmd,-35}  -  {Description}";
        }

        public void SendMessageToPlayer(Optional<Player> player, string message)
        {
            if (player.HasValue)
            {
                player.Value.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
            }
        }

        public void Notify(Optional<Player> player, string message)
        {
            Log.Info(message);
            SendMessageToPlayer(player, message);
        }
    }
}
