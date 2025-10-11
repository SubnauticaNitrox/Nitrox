using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlaceModule : Packet
{
    public ModuleEntity ModuleEntity { get; }

    public PlaceModule(ModuleEntity moduleEntity)
    {
        ModuleEntity = moduleEntity;
    }
}
