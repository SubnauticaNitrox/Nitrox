using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity)
        {
            Log.Debug("Adding Crash entity to CrashHome...");

            Entity childEntity = new Entity(parentEntity.Position, parentEntity.Rotation, TechType.Crash, parentEntity.Level, parentEntity.ClassId);

            parentEntity.ChildEntity = Optional<Entity>.Of(childEntity);
        }
    }
}
