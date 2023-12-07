namespace NitroxModel.DataStructures.GameLogic.Entities;

public interface IUweWorldEntityFactory
{
    public abstract bool TryFind(string classId, out UweWorldEntity uweWorldEntity);
}
