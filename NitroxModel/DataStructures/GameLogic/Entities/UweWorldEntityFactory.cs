namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public abstract class UweWorldEntityFactory
    {
        public abstract bool TryFind(string classId, out UweWorldEntity uweWorldEntity);
    }
}
