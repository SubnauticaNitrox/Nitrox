using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Resources;

public class PrefabPlaceholderAsset
{
    public string ClassId { get; }
    
    public NitroxEntitySlot EntitySlot { get; }

    public PrefabPlaceholderAsset(string classId, NitroxEntitySlot entitySlot)
    {
        ClassId = classId;
        EntitySlot = entitySlot;
    }
}
