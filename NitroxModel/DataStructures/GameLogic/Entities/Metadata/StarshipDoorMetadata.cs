using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class StarshipDoorMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool DoorLocked { get; }
    [JsonMemberTransition]
    public bool DoorOpen { get; }

    public StarshipDoorMetadata(bool doorLocked, bool doorOpen)
    {
        DoorLocked = doorLocked;
        DoorOpen = doorOpen;
    }

    public override string ToString()
    {
        return $"[StarshipDoorMetadata - DoorLocked: {DoorLocked} DoorOpen: {DoorOpen}]";
    }
}
