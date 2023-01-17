using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class GlobalRootInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Entities entities;

        public GlobalRootInitialSyncProcessor(Entities entities)
        {
            this.entities = entities;

            // As we migrate systems over to entities, we want to ensure the required components are in place to spawn these entities.
            // For example, migrating inventories to the entity system requires players are spawned in the world before we try to add
            // inventory items to them.  Eventually, all of the below processors will become entities on their own 
            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor));
            DependentProcessors.Add(typeof(RemotePlayerInitialSyncProcessor));
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            Log.Info($"Received initial sync packet with {packet.GlobalRootEntities.Count} global root entities");
            yield return entities.SpawnAsync(packet.GlobalRootEntities);
        }
    }
}
