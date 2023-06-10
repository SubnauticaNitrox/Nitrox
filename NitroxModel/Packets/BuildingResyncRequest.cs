using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

public class BuildingResyncRequest : Packet
{
    public NitroxId EntityId { get; set; }
    public bool ResyncEverything { get; set; }

    public BuildingResyncRequest(NitroxId entityId)
    {
        EntityId = entityId;
    }

    public BuildingResyncRequest()
    {
        ResyncEverything = true;
    }
}
