using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

public class UweWorldEntity
{
    public string ClassId { get; }
    public NitroxTechType TechType { get; }
    public string SlotType { get; }
    public bool PrefabZUp { get; }
    public int CellLevel { get; }
    public NitroxVector3 LocalScale { get; }

    public UweWorldEntity(string classId, NitroxTechType techType, string slotType, bool prefabZUp, int cellLevel, NitroxVector3 localScale)
    {
        ClassId = classId;
        TechType = techType;
        SlotType = slotType;
        PrefabZUp = prefabZUp;
        CellLevel = cellLevel;
        LocalScale = localScale;
    }
}
