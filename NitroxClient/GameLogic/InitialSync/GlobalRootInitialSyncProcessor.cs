using System.Collections;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

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
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            yield return new WaitUntil(LargeWorldStreamer.main.IsWorldSettled);
            // Make sure all building-related prefabs are fully loaded (happen to bug when launching multiple clients locally)
            yield return Base.InitializeAsync();
            yield return BaseGhost.InitializeAsync();
            yield return BaseDeconstructable.InitializeAsync();

            BuildingHandler.Main.InitializeOperations(packet.BuildOperationIds);

            Log.Info($"Received initial sync packet with {packet.GlobalRootEntities.Count} global root entities");
            foreach (Entity entity in packet.GlobalRootEntities)
            {
                Log.Debug($"Going to spawn: {entity.Id} of type {entity.GetType()}");
            }
            yield return entities.SpawnAsync(packet.GlobalRootEntities);
        }
    }
}
