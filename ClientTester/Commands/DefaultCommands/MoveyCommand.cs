using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class MoveyCommand : NitroxCommand
    {
        private readonly PlayerLogic playerBroadcaster = NitroxServiceLocator.LocateService<PlayerLogic>();

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
            playerBroadcaster.UpdateLocation(client.ClientPos, new Vector3(0, client.ClientPos.y - y, 0), Quaternion.identity, Quaternion.identity, Optional<VehicleModel>.Empty(), Optional<string>.Empty());
        }
    }
}
