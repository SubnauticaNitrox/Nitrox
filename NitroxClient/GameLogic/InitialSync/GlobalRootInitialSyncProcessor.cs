using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class GlobalRootInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Entities entities;

        public GlobalRootInitialSyncProcessor(Entities entities)
        {
            this.entities = entities;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            Log.Info($"Received initial sync packet with {packet.GlobalRootEntities.Count} global root entities");
            List<WorldEntity> worldEntities = packet.GlobalRootEntities.Cast<WorldEntity>().ToList();
            yield return entities.SpawnAsync(worldEntities);
        }
    }
}
