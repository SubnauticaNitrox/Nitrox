using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Packets;

public sealed class PlaceModule : Packet
{
    public ModuleEntity ModuleEntity;

    public PlaceModule(ModuleEntity moduleEntity)
    {
        ModuleEntity = moduleEntity;
    }

    public override string ToString()
    {
        return $"PlaceModule [ModuleEntity: {ModuleEntity}]";
    }
}
