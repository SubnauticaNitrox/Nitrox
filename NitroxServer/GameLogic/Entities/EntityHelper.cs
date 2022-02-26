using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities;

public abstract class EntityHelper
{
    public abstract bool TryGetTechTypeForClassId(string classId, out NitroxTechType nitroxTechType);
}
