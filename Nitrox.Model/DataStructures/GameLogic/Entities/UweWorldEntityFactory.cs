namespace Nitrox.Model.DataStructures.GameLogic.Entities
{
    public abstract class UweWorldEntityFactory
    {
        public abstract Optional<UweWorldEntity> From(string classId);
    }
}
