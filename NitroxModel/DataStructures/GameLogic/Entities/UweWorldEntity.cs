namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public class UweWorldEntity
    {
        public NitroxTechType TechType { get; }
        public NitroxVector3 Scale { get; }
        public string ClassId { get; }
        public string SlotType { get; }
        public int CellLevel { get; }

        public UweWorldEntity(NitroxTechType techType, NitroxVector3 scale, string classId, string slotType, int cellLevel)
        {
            TechType = techType;
            Scale = scale;
            ClassId = classId;
            SlotType = slotType;
            CellLevel = cellLevel;
        }

        public override string ToString()
        {
            return $"[UweWorldEntity - TechType: {TechType}, Scale: {Scale}, ClassId: {ClassId}, SlotType: {SlotType}, CellLevel: {CellLevel}]";
        }
    }
}
