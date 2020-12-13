using System.Collections;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic.InitialSync
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
