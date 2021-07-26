using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class EscapePodInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodInitialSyncProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            EscapePodModel escapePod = packet.EscapePodsData.Find(x => x.Id.Equals(packet.AssignedEscapePodId));
            
            escapePodManager.AssignPlayerToEscapePod(escapePod);
            yield return null;

            escapePodManager.SyncEscapePodIds(packet.EscapePodsData);
        }
    }
}
