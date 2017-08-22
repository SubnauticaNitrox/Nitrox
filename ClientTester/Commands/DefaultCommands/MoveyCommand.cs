using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveyCommand : NitroxCommand
    {
        public MoveyCommand()
        {
            Name = "movey";
            Description = "Moves the player up and down";
            Syntax = "movey <y>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            if (args.Length < 1)
            {
                throw new NotEnoughArgumentsException(1);
            }

            client.clientPos.y = float.Parse(args[0]);
            client.PacketSender.UpdatePlayerLocation(client.clientPos, Quaternion.identity, Quaternion.identity, Optional<VehicleModel>.Empty(), Optional<String>.Empty());
        }
    }
}
