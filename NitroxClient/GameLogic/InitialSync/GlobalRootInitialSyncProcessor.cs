using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
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
            yield return entities.SpawnAsync(packet.GlobalRootEntities.Cast<Entity>().ToList());
        }
    }
}
