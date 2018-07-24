using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveyCommand : NitroxCommand
    {
        private readonly LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

        public MoveyCommand()
        {
            Name = "movey";
            Description = "Moves the player up and down";
            Syntax = "movey <y>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            assertMinimumArgs(args, 1);

            float y = client.ClientPos.y;
            client.ClientPos.y = float.Parse(args[0]);
            localPlayer.UpdateLocation(client.ClientPos, new Vector3(0, client.ClientPos.y - y, 0), Quaternion.identity, Quaternion.identity, Optional<VehicleMovementData>.Empty(), Optional<string>.Empty());
        }
    }
}
