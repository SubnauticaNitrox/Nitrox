namespace NitroxModel.DataStructures.GameLogic.Entities;

public interface IUweWorldEntityFactory
{
    public bool TryFind(string classId, out UweWorldEntity uweWorldEntity);
}
