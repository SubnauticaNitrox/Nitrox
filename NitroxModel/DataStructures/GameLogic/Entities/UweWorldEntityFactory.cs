using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public abstract class UweWorldEntityFactory
    {
        public abstract Optional<UweWorldEntity> From(string classId);
    }
}
