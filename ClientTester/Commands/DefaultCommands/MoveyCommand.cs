using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveyCommand : NitroxCommand
    {
        public override void Execute(MultiplayerClient client, string[] args)
        {
            Name = "movey";
            Description = "Moves the player up and down";
            Syntax = "movey <y>";
            if (args.Length < 1)
            {
                CommandManager.NotEnoughArgumentsMessage(1, Syntax);
                return;
            }

            client.clientPos.y = float.Parse(args[0]);
            client.PacketSender.UpdatePlayerLocation(client.clientPos, Quaternion.identity, Optional<VehicleModel>.Empty());
        }
    }
}
