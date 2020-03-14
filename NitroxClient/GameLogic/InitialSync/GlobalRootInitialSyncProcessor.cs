using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Logger;
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
            Log.Info("Received initial sync packet with " + packet.GlobalRootEntities.Count + " global root entities");
            entities.Spawn(packet.GlobalRootEntities);
            yield return null;
        }
    }
}
