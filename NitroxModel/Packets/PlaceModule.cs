using System;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

[Serializable]
public sealed class PlaceModule : Packet
{
    public ModuleEntity ModuleEntity { get; }

    public PlaceModule(ModuleEntity moduleEntity)
    {
        ModuleEntity = moduleEntity;
    }
}
