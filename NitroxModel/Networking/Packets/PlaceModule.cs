using System;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record PlaceModule : Packet
{
    public ModuleEntity ModuleEntity { get; }

    public PlaceModule(ModuleEntity moduleEntity)
    {
        ModuleEntity = moduleEntity;
    }
}
