namespace NitroxModel.DataStructures.GameLogic.Entities;

public interface IUweWorldEntityFactory
{
    public bool TryFindAsync(string classId, out UweWorldEntity uweWorldEntity);
}
