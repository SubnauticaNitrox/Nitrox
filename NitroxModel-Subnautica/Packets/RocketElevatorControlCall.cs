using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using static Rocket;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketElevatorControlCall : Packet
    {
        public NitroxId Id { get; }
        public Rocket.RocketElevatorStates ElevatorState { get; }
        public bool Up => ElevatorState == RocketElevatorStates.Up || ElevatorState == RocketElevatorStates.AtTop;

        public RocketElevatorControlCall(NitroxId id, Rocket.RocketElevatorStates rocketElevatorState)
        {
            Id = id;
            ElevatorState = rocketElevatorState;
        }

        public override string ToString()
        {
            return $"[RocketElevatorControlCall - RocketId: {Id}, ElevatorState: {ElevatorState}]";
        }
    }
}

