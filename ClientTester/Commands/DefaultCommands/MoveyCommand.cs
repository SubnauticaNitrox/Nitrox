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
            assertMinimumArgs(args, 1);

            float y = client.clientPos.y;
            client.clientPos.y = float.Parse(args[0]);
            client.PacketSender.UpdatePlayerLocation(client.clientPos, new Vector3(0, client.clientPos.y - y, 0), Quaternion.identity, Quaternion.identity, Optional<VehicleModel>.Empty(), Optional<String>.Empty());
        }
    }
}
